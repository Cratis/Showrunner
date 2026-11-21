// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.Channels;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Checks;

/// <summary>
/// Probes public URLs the way a user reaches the product. Everything inside the cluster can be green while a load balancer
/// still routes a share of connections to a dead backend, which a plain TCP health check does not see. Each URL is tried three
/// times, a second apart, because a round-robin across one dead backend is a blip, not an outage. A 5xx or no answer is a
/// failure; a 4xx, such as a 401 from a protected page, means the product answered.
/// </summary>
/// <param name="client">The HTTP client used to probe.</param>
/// <param name="urls">The URLs.</param>
/// <param name="timeProvider">The clock, which bounds each attempt and spaces them.</param>
public sealed class PublicUrlsCheck(HttpClient client, IEnumerable<Uri> urls, TimeProvider timeProvider) : IWatchdogCheck
{
    /// <summary>
    /// The name of the check in configuration.
    /// </summary>
    public const string CheckName = "public-urls";

    const int Attempts = 3;

    static readonly TimeSpan _attemptTimeout = TimeSpan.FromSeconds(6);
    static readonly TimeSpan _attemptPause = TimeSpan.FromSeconds(1);

    readonly IReadOnlyList<Uri> _urls = [.. urls];

    /// <inheritdoc/>
    public string Name => CheckName;

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<Finding>> Collect(CheckContext context, CancellationToken cancellationToken)
    {
        var findings = new List<Finding>();
        foreach (var url in _urls)
        {
            var failures = new List<string>();
            var attempts = 0;
            for (var attempt = 0; attempt < Attempts; attempt++)
            {
                var needed = _attemptTimeout + (attempt > 0 ? _attemptPause : TimeSpan.Zero);
                if (context.Remaining < needed)
                {
                    // A partial probe is not a clean one: an intermittent backend needs all three attempts.
                    findings.Add(new Finding($"watchdog:budget:url:{url}", $"{url} probed {attempts}/{Attempts} attempts; the rest were skipped: collection budget exhausted", AlertSeverity.Warning));
                    break;
                }

                if (attempt > 0)
                {
                    await Task.Delay(_attemptPause, timeProvider, cancellationToken);
                }

                attempts++;
                if (await Attempt(url, cancellationToken) is { } failure)
                {
                    failures.Add(failure);
                }
            }

            if (failures.Count > 0)
            {
                findings.Add(new Finding($"url:{url}", $"{url} failed {failures.Count}/{attempts} attempts: {string.Join("; ", failures)}", AlertSeverity.Critical));
            }
        }

        return findings;
    }

    async Task<string?> Attempt(Uri url, CancellationToken cancellationToken)
    {
        using var timeout = new CancellationTokenSource(_attemptTimeout, timeProvider);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.ParseAdd("text/html");
            request.Headers.UserAgent.ParseAdd(DiscordAlertChannel.UserAgent);
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, linked.Token);
            return (int)response.StatusCode >= 500 ? $"HTTP {(int)response.StatusCode}; inspect app and ingress health" : null;
        }
        catch (Exception error) when (!cancellationToken.IsCancellationRequested)
        {
            return error is OperationCanceledException ? $"no answer within {(int)_attemptTimeout.TotalSeconds}s" : Redaction.Summarize(error.Message);
        }
    }
}
