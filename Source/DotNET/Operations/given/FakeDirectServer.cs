// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Cryptography;
using System.Text.Json;
using Cratis.Showrunner.Operations.Link.Protocol;

namespace Cratis.Showrunner.Operations.given;

/// <summary>
/// A small Direct that speaks the real watchdog protocol with the semantics of Direct's own gateway: a one-time enrollment token
/// exchanged for an instance credential, a fencing epoch with <c>409</c> for a stale one or a live clone, signed configuration
/// with a lease, an inbox that deduplicates by sequence and reports gaps, and remedy requests that are authorized on demand.
/// It stands in for Direct in specs (through <see cref="FakeDirectHandler"/>) and in the end-to-end run.
/// </summary>
/// <param name="instance">The instance name it serves.</param>
/// <param name="enrollmentToken">The one-time enrollment token.</param>
/// <param name="timeProvider">The clock.</param>
public sealed class FakeDirectServer(string instance, string enrollmentToken, TimeProvider timeProvider)
{
    readonly object _lock = new();
    readonly List<WatchdogRemedyRequest> _requests = [];
    readonly List<Guid> _authorized = [];
    string _credential = string.Empty;
    string _holder = string.Empty;
    DateTimeOffset _leaseExpires;
    WatchdogConfigurationSnapshot? _published;

    /// <summary>
    /// Gets the signing key.
    /// </summary>
    public string SigningKey { get; private set; } = ConfigurationSignature.NewKey();

    /// <summary>
    /// Gets the current fencing epoch, zero before the first claim.
    /// </summary>
    public long Epoch { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether the server is unreachable.
    /// </summary>
    public bool Unreachable { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether Direct takes event batches but the answers never arrive.
    /// </summary>
    public bool DropResponses { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether delivered configuration carries a signature that does not verify.
    /// </summary>
    public bool TamperSignature { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether every recorded remedy request is authorized.
    /// </summary>
    public bool AutoAuthorize { get; set; }

    /// <summary>
    /// Gets or sets how long a delivered configuration lasts.
    /// </summary>
    public TimeSpan ConfigurationLease { get; set; } = TimeSpan.FromHours(1);

    /// <summary>
    /// Gets the instance credential once claimed.
    /// </summary>
    public string Credential => _credential;

    /// <summary>
    /// Gets how many claims were answered successfully.
    /// </summary>
    public int Claims { get; private set; }

    /// <summary>
    /// Gets the events the inbox took, in sequence.
    /// </summary>
    public List<WatchdogEventEnvelope> Events { get; } = [];

    /// <summary>
    /// Gets how many envelopes arrived that had already been taken.
    /// </summary>
    public int Duplicates { get; private set; }

    /// <summary>
    /// Gets the gaps the inbox recorded.
    /// </summary>
    public List<WatchdogSequenceGap> Gaps { get; } = [];

    /// <summary>
    /// Gets the highest sequence taken.
    /// </summary>
    public long Watermark { get; private set; }

    /// <summary>
    /// Gets the revisions acknowledged by the instance.
    /// </summary>
    public List<WatchdogConfigurationAcknowledgement> Acknowledged { get; } = [];

    /// <summary>
    /// Gets the remedy requests recorded.
    /// </summary>
    public IReadOnlyList<WatchdogRemedyRequest> RemedyRequests => _requests;

    /// <summary>
    /// Gets the bodies of the alert webhook deliveries.
    /// </summary>
    public List<string> AlertWebhooks { get; } = [];

    /// <summary>
    /// Gets the requests received, as <c>METHOD path</c>.
    /// </summary>
    public List<string> Received { get; } = [];

    /// <summary>
    /// Gets the names of the checks Direct would hold an alert open for: failed at least twice in a row and not recovered.
    /// </summary>
    public IReadOnlyCollection<string> OpenAlerts
    {
        get
        {
            var open = new HashSet<string>(StringComparer.Ordinal);
            foreach (var envelope in Events)
            {
                if (envelope.PayloadAs<CheckFailed>() is { ConsecutiveFailures: >= 2 } failed)
                {
                    open.Add(failed.Check);
                }
                else if (envelope.PayloadAs<CheckRecovered>() is { } recovered)
                {
                    open.Remove(recovered.Check);
                }
            }

            return open;
        }
    }

    /// <summary>
    /// Publishes a new configuration revision.
    /// </summary>
    /// <param name="checks">The checks.</param>
    /// <param name="remedies">The remedies.</param>
    /// <param name="offline">The offline policy.</param>
    /// <param name="secrets">The secrets.</param>
    /// <returns>The revision.</returns>
    public long Publish(IReadOnlyList<EndpointCheckDefinition> checks, IReadOnlyList<RemedyDefinition> remedies, OfflinePolicy offline, IReadOnlyDictionary<string, string>? secrets = null)
    {
        lock (_lock)
        {
            var now = timeProvider.GetUtcNow();
            var snapshot = new WatchdogConfigurationSnapshot(instance, (_published?.Revision ?? 0) + 1, now, now, string.Empty, checks, remedies, secrets ?? new Dictionary<string, string>(), offline);
            _published = snapshot with { Digest = ConfigurationDigest.Of(snapshot) };
            return _published.Revision;
        }
    }

    /// <summary>
    /// Rotates the signing key, as an operator might.
    /// </summary>
    public void RotateKey() => SigningKey = ConfigurationSignature.NewKey();

    /// <summary>
    /// Takes over the instance by another session, as a copied container would.
    /// </summary>
    /// <param name="session">The other session.</param>
    public void TakeOver(string session)
    {
        lock (_lock)
        {
            _holder = session;
            Epoch++;
            _leaseExpires = timeProvider.GetUtcNow() + WatchdogProtocol.LeaseDuration;
        }
    }

    /// <summary>
    /// Handles one request.
    /// </summary>
    /// <param name="method">The HTTP method.</param>
    /// <param name="path">The path.</param>
    /// <param name="query">The query string including the leading question mark, or empty.</param>
    /// <param name="bearer">The bearer token, or <see langword="null"/>.</param>
    /// <param name="epoch">The epoch header, or <see langword="null"/>.</param>
    /// <param name="body">The body.</param>
    /// <returns>The status code and body.</returns>
    public (int Status, string Body) Handle(string method, string path, string query, string? bearer, string? epoch, string body)
    {
        lock (_lock)
        {
            Received.Add($"{method} {path}");
            var prefix = WatchdogProtocol.For(WatchdogProtocol.Base, instance);
            if (!path.StartsWith(prefix, StringComparison.Ordinal))
            {
                AlertWebhooks.Add(body);
                return (200, "{}");
            }

            var route = path[prefix.Length..];
            return route == "/claim" ? Claim(bearer, body) : Authenticated(route, query, bearer, epoch, body);
        }
    }

    static (int Status, string Body) Json<T>(T value) => (200, JsonSerializer.Serialize(value, WatchdogJson.Options));

    (int Status, string Body) Claim(string? bearer, string body)
    {
        var request = JsonSerializer.Deserialize<WatchdogClaimRequest>(body, WatchdogJson.Options);
        if (request is null || string.IsNullOrEmpty(request.Session))
        {
            return (400, "a claim names its runtime version and session");
        }

        var now = timeProvider.GetUtcNow();
        var first = _credential.Length == 0;
        if (bearer != (first ? enrollmentToken : _credential))
        {
            return (401, "not accepted");
        }

        if (!first && request.Session != _holder)
        {
            if (now < _leaseExpires)
            {
                return (409, "another session holds the instance");
            }

            Epoch++;
        }

        if (first)
        {
            _credential = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
            Epoch = 1;
        }

        _holder = request.Session;
        _leaseExpires = now + WatchdogProtocol.LeaseDuration;
        Claims++;
        return Json(new WatchdogClaimResponse(instance, $"watchdog-{instance}", Epoch, first ? _credential : string.Empty, SigningKey, _leaseExpires));
    }

    (int Status, string Body) Authenticated(string route, string query, string? bearer, string? epoch, string body)
    {
        if (bearer != _credential || _credential.Length == 0)
        {
            return (401, "not accepted");
        }

        if (epoch != Epoch.ToString(System.Globalization.CultureInfo.InvariantCulture))
        {
            return (409, "stale epoch");
        }

        _leaseExpires = timeProvider.GetUtcNow() + WatchdogProtocol.LeaseDuration;
        return route switch
        {
            "/configuration" => Configuration(query),
            "/configuration/applied" => Applied(body),
            "/events" => Receive(body),
            "/remedies/requests" => Request(body),
            "/remedies/authorizations" => Authorizations(),
            _ => (404, string.Empty)
        };
    }

    (int Status, string Body) Configuration(string query)
    {
        var parameters = System.Web.HttpUtility.ParseQueryString(query);
        var after = long.TryParse(parameters[WatchdogProtocol.AfterParameter], out var revision) ? revision : 0;
        var renew = bool.TryParse(parameters[WatchdogProtocol.RenewParameter], out var renewing) && renewing;
        if (_published is null || (after >= _published.Revision && !renew))
        {
            return (204, string.Empty);
        }

        var now = timeProvider.GetUtcNow();
        var signed = ConfigurationSignature.Sign(_published with { IssuedAt = now, ExpiresAt = now + ConfigurationLease }, SigningKey);
        return Json(TamperSignature ? signed with { Signature = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)) } : signed);
    }

    (int Status, string Body) Applied(string body)
    {
        var acknowledgement = JsonSerializer.Deserialize<WatchdogConfigurationAcknowledgement>(body, WatchdogJson.Options)!;
        if (_published is null || acknowledgement.Revision != _published.Revision || acknowledgement.Digest != _published.Digest)
        {
            return (400, "not the published revision");
        }

        Acknowledged.Add(acknowledgement);
        return Json(acknowledgement);
    }

    (int Status, string Body) Receive(string body)
    {
        var batch = JsonSerializer.Deserialize<WatchdogEventBatch>(body, WatchdogJson.Options)!;
        if (batch.Events.Count > WatchdogProtocol.MaximumBatchSize)
        {
            return (400, $"a batch carries at most {WatchdogProtocol.MaximumBatchSize} events");
        }

        var accepted = 0;
        var duplicates = 0;
        var gaps = new List<WatchdogSequenceGap>();
        foreach (var envelope in batch.Events.OrderBy(entry => entry.Sequence))
        {
            if (envelope.Sequence <= Watermark)
            {
                duplicates++;
                continue;
            }

            if (envelope.Sequence > Watermark + 1)
            {
                gaps.Add(new WatchdogSequenceGap(Watermark + 1, envelope.Sequence - 1));
            }

            Events.Add(envelope);
            Watermark = envelope.Sequence;
            accepted++;
        }

        Duplicates += duplicates;
        Gaps.AddRange(gaps);
        return Json(new WatchdogEventBatchReceipt(Watermark, accepted, duplicates, 0, gaps));
    }

    (int Status, string Body) Request(string body)
    {
        var request = JsonSerializer.Deserialize<WatchdogRemedyRequest>(body, WatchdogJson.Options)!;
        var recorded = _requests.TrueForAll(existing => existing.Request != request.Request);
        if (recorded)
        {
            _requests.Add(request);
        }

        return Json(new WatchdogRemedyRequestReceipt(request.Request, recorded));
    }

    (int Status, string Body) Authorizations()
    {
        if (AutoAuthorize)
        {
            _authorized.AddRange(_requests.Select(request => request.Request).Except(_authorized));
        }

        var now = timeProvider.GetUtcNow();
        var granted = _requests
            .Where(request => _authorized.Contains(request.Request))
            .Select(request => new WatchdogRemedyAuthorization(
                request.Request,
                request.Remedy,
                _published?.Remedies.FirstOrDefault(remedy => remedy.Name == request.Remedy)?.Resource ?? Guid.Empty,
                request.ConfigurationRevision,
                Epoch,
                now + TimeSpan.FromMinutes(10)))
            .ToList();
        return Json(new WatchdogRemedyAuthorizations(granted));
    }
}
#endif
