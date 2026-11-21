// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Operations.Alerts;

namespace Cratis.Showrunner.Operations.Channels;

/// <summary>
/// Pings a Healthchecks.io style check: a POST to the check URL on success, to <c>/fail</c> on failure. A ping is tried twice.
/// The URL is a capability, so no error ever carries it.
/// </summary>
/// <param name="client">The HTTP client; its timeout bounds each attempt.</param>
/// <param name="checkUrl">The URL of the check.</param>
public sealed class HealthchecksHeartbeat(HttpClient client, Uri checkUrl) : IHeartbeat
{
    const int Attempts = 2;

    /// <inheritdoc/>
    public async Task Ping(bool failed, CancellationToken cancellationToken)
    {
        var url = new Uri(checkUrl.ToString().TrimEnd('/') + (failed ? "/fail" : string.Empty));
        for (var attempt = 0; attempt < Attempts; attempt++)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = new ByteArrayContent([]) };
                using var response = await client.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();
                return;
            }
            catch (Exception) when (!cancellationToken.IsCancellationRequested)
            {
                // The transport error is dropped: it can contain the capability URL.
            }
        }

        throw new InvalidOperationException("external heartbeat delivery failed after two attempts");
    }
}
