// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Operations.Alerts;

namespace Cratis.Showrunner.Operations.Channels;

/// <summary>
/// Delivers alerts to a Discord webhook as one embed.
/// </summary>
/// <param name="client">The HTTP client; its timeout bounds the delivery.</param>
/// <param name="webhookUrl">The webhook URL. It is a capability: it is never logged or put in an exception.</param>
public sealed class DiscordAlertChannel(HttpClient client, Uri webhookUrl) : IAlertChannel
{
    /// <summary>
    /// The user agent the webhook is called with. Cloudflare in front of Discord rejects the default agent strings of some
    /// runtimes with 403.
    /// </summary>
    public const string UserAgent = "cratis-cluster-health-watchdog/1.0";

    /// <summary>
    /// Discord caps an embed description at 4096 characters.
    /// </summary>
    const int DescriptionLimit = 4000;
    const int Red = 0xE74C3C;
    const int Orange = 0xE67E22;
    const int Blue = 0x3498DB;

    /// <inheritdoc/>
    public string Name => "discord";

    /// <inheritdoc/>
    public async Task Deliver(AlertNotification notification, CancellationToken cancellationToken)
    {
        var body = notification.Body;
        var payload = new
        {
            embeds = new[]
            {
                new
                {
                    title = notification.Subject,
                    description = body.Length <= DescriptionLimit ? body : body[..DescriptionLimit],
                    color = notification.Severity switch
                    {
                        AlertSeverity.Critical => Red,
                        AlertSeverity.Warning => Orange,
                        _ => Blue
                    }
                }
            }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, webhookUrl) { Content = System.Net.Http.Json.JsonContent.Create(payload) };
        request.Headers.UserAgent.ParseAdd(UserAgent);
        using var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
