// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net.Http.Headers;
using Cratis.Showrunner.Operations.Alerts;

namespace Cratis.Showrunner.Operations.Channels;

/// <summary>
/// Pushes alerts to Cratis Direct's alert webhook, <c>POST /webhooks/alerts</c>, one delivery per finding. The finding's key is
/// Direct's fingerprint, so a finding that is alerted again, or escalated, is the same alert in Direct rather than a new one,
/// and one whose severity changed is recorded as a change. Direct's webhook only raises; it has no way to close an alert, so
/// a resolved finding is not pushed — the alert stays open in Direct until it is resolved there, and a recurrence reopens it.
/// </summary>
/// <param name="client">The HTTP client; its timeout bounds each delivery.</param>
/// <param name="options">Where to push and as whom.</param>
public sealed class DirectAlertChannel(HttpClient client, DirectAlertsOptions options) : IAlertChannel
{
    /// <summary>
    /// The path of Direct's alert webhook.
    /// </summary>
    public const string Path = "/webhooks/alerts";

    const int TitleLimit = 200;
    const string TestFingerprint = "watchdog:test-alert";

    /// <inheritdoc/>
    public string Name => "direct";

    /// <inheritdoc/>
    public async Task Deliver(AlertNotification notification, CancellationToken cancellationToken)
    {
        foreach (var finding in notification.Raised)
        {
            await Push(notification, finding, cancellationToken);
        }

        // An escalated finding has been open for a day: it is pushed as critical, which Direct records as a change of severity.
        foreach (var finding in notification.Escalated)
        {
            await Push(notification, finding with { Severity = AlertSeverity.Critical }, cancellationToken);
        }
    }

    static string Severity(AlertSeverity severity) => severity switch
    {
        AlertSeverity.Critical => "critical",
        AlertSeverity.Warning => "warning",
        _ => "information"
    };

    async Task Push(AlertNotification notification, Finding finding, CancellationToken cancellationToken)
    {
        var title = $"[{notification.Deployment}] {finding.Key}";
        var payload = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["title"] = title.Length <= TitleLimit ? title : title[..TitleLimit],
            ["summary"] = finding.Text,
            ["severity"] = Severity(notification.IsTest ? AlertSeverity.Information : finding.Severity),
            ["fingerprint"] = notification.IsTest ? TestFingerprint : finding.Key,
            ["labels"] = new Dictionary<string, string>(StringComparer.Ordinal) { ["deployment"] = notification.Deployment }
        };
        if (!string.IsNullOrWhiteSpace(options.Source))
        {
            payload["source"] = options.Source;
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(options.BaseUrl.ToString().TrimEnd('/') + Path)) { Content = System.Net.Http.Json.JsonContent.Create(payload) };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.Token);
        using var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
