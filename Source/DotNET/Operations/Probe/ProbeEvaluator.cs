// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net.Sockets;
using System.Text;

namespace Cratis.Showrunner.Operations.Probe;

/// <summary>
/// Calls the public endpoints the way an anonymous visitor does and says which ones are down. Every request is made in the first
/// round; the ones that failed are made again once, after a pause, in the second. A response body is never read, so an endpoint that
/// streams (server-sent events) cannot hold the probe. The HTTP client must not follow redirects: a redirect is an answer, not a pass.
/// </summary>
/// <param name="http">The HTTP client, with automatic redirects turned off.</param>
/// <param name="timeProvider">The clock.</param>
public sealed class ProbeEvaluator(HttpClient http, TimeProvider timeProvider)
{
    /// <summary>
    /// How long a request may take unless the check says otherwise.
    /// </summary>
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);

    /// <summary>
    /// The longest pause between the rounds.
    /// </summary>
    public static readonly TimeSpan MaximumRoundGap = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Replaces the base address of the targets that are named.
    /// </summary>
    /// <param name="targets">The targets.</param>
    /// <param name="baseUrls">The new base addresses by target name.</param>
    /// <returns>The targets with their addresses replaced.</returns>
    public static IReadOnlyList<ProbeTarget> Override(IEnumerable<ProbeTarget> targets, IReadOnlyDictionary<string, string> baseUrls) =>
        [.. targets.Select(target => baseUrls.TryGetValue(target.Name, out var url) ? target with { BaseUrl = url } : target)];

    /// <summary>
    /// Probes the targets.
    /// </summary>
    /// <param name="targets">The targets.</param>
    /// <param name="roundGap">How long to wait before the second round.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The report.</returns>
    public async Task<ProbeReport> Evaluate(IReadOnlyList<ProbeTarget> targets, TimeSpan roundGap, CancellationToken cancellationToken)
    {
        var entries = targets.SelectMany(target => target.Checks.Select(endpoint => (Target: target, Endpoint: endpoint))).ToList();
        var first = await Task.WhenAll(entries.Select(entry => Check(entry.Target, entry.Endpoint, cancellationToken)));
        var retry = entries.Where((_, index) => !first[index].Ok).ToList();
        if (retry.Count == 0)
        {
            return new ProbeReport([first], [], []);
        }

        await Task.Delay(roundGap, timeProvider, cancellationToken);
        var second = await Task.WhenAll(retry.Select(entry => Check(entry.Target, entry.Endpoint, cancellationToken)));
        return new ProbeReport([first, second], [.. second.Where(result => !result.Ok)], [.. second.Where(result => result.Ok)]);
    }

    /// <summary>
    /// Makes one request.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <param name="endpoint">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>What the endpoint answered.</returns>
    public async Task<ProbeResult> Check(ProbeTarget target, ProbeEndpoint endpoint, CancellationToken cancellationToken)
    {
        var started = timeProvider.GetTimestamp();
        int? status = null;
        string? error = null;
        try
        {
            using var limit = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            limit.CancelAfter(endpoint.TimeoutMs is { } timeout ? TimeSpan.FromMilliseconds(timeout) : DefaultTimeout);
            using var request = new HttpRequestMessage(new HttpMethod(endpoint.Method), new Uri($"{target.BaseUrl.TrimEnd('/')}{endpoint.Path}"));
            request.Headers.Accept.ParseAdd(endpoint.Accept);
            request.Headers.Add("Sec-Fetch-Dest", "empty");
            request.Headers.UserAgent.ParseAdd("cratis-synthetic-probe/1");
            if (endpoint.Body is not null)
            {
                request.Content = new StringContent(endpoint.Body, Encoding.UTF8, "application/json");
            }

            // Headers only, and the response is disposed unread: even an accidental anonymous 200 may stream forever.
            using var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, limit.Token);
            status = (int)response.StatusCode;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            error = "TimeoutError";
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            error = ErrorCode(exception);
        }

        return new ProbeResult(target.Name, endpoint.Id, status == endpoint.ExpectStatus, status, error, timeProvider.GetElapsedTime(started).TotalMilliseconds);
    }

    /// <summary>
    /// Maps a failure to a stable error code. Only the code is kept: exception messages can contain URLs.
    /// </summary>
    /// <param name="exception">The failure.</param>
    /// <returns>The error code.</returns>
    static string ErrorCode(Exception exception) => exception switch
    {
        HttpRequestException { InnerException: SocketException socket } => socket.SocketErrorCode.ToString(),
        HttpRequestException { HttpRequestError: not HttpRequestError.Unknown } http => http.HttpRequestError.ToString(),
        _ => "RequestError"
    };
}
