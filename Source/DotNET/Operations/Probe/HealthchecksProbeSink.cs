// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;

namespace Cratis.Showrunner.Operations.Probe;

/// <summary>
/// Reports a probe to a Healthchecks check: a plain ping when every endpoint answers, and a <c>/fail</c> ping carrying the failing
/// endpoints when one is down. Pings are idempotent, so a delivery is attempted four times. The ping address is a capability
/// and is never part of an error.
/// </summary>
/// <param name="http">The HTTP client; it must not follow redirects.</param>
/// <param name="pingUrl">The ping address of the check.</param>
public sealed class HealthchecksProbeSink(HttpClient http, Uri pingUrl)
{
    /// <summary>
    /// How many times a ping is attempted: the first time and three retries.
    /// </summary>
    public const int Attempts = 4;

    /// <summary>
    /// The most of the body Healthchecks keeps.
    /// </summary>
    public const int MaximumBodyBytes = 9900;

    /// <summary>
    /// Reports the report.
    /// </summary>
    /// <param name="report">The report.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the report is delivered.</returns>
    /// <exception cref="InvalidOperationException">Every attempt failed.</exception>
    public async Task Report(ProbeReport report, CancellationToken cancellationToken)
    {
        var down = report.IsDown;
        var endpoint = down ? new Uri($"{pingUrl.AbsoluteUri.TrimEnd('/')}/fail") : pingUrl;
        var body = Encoding.UTF8.GetBytes(report.FailingText);
        for (var attempt = 0; attempt < Attempts; attempt++)
        {
            try
            {
                using var request = new HttpRequestMessage(down ? HttpMethod.Post : HttpMethod.Get, endpoint);
                if (down)
                {
                    request.Content = new ByteArrayContent(body, 0, Math.Min(body.Length, MaximumBodyBytes));
                }

                using var response = await http.SendAsync(request, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch (Exception error) when (error is not OperationCanceledException)
            {
                // The error is not kept: it may contain the capability URL.
            }
        }

        throw new InvalidOperationException("healthchecks delivery failed");
    }
}
