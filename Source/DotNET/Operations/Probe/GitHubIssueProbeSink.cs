// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;

namespace Cratis.Showrunner.Operations.Probe;

/// <summary>
/// Keeps one GitHub issue per outage: it opens an issue labeled <c>probe-down</c> when an endpoint is down, comments on the open one
/// while it stays down, and comments and closes it when everything answers again. It uses the REST API with a token that may write
/// issues; the token and the response bodies are never part of an error.
/// </summary>
/// <param name="http">The HTTP client; it must not follow redirects.</param>
/// <param name="repository">The repository, as <c>owner/name</c>.</param>
/// <param name="token">The token.</param>
public sealed class GitHubIssueProbeSink(HttpClient http, string repository, string token)
{
    /// <summary>
    /// The label that marks the issue of an outage.
    /// </summary>
    public const string Label = "probe-down";

    static readonly Uri _api = new("https://api.github.com/");

    /// <summary>
    /// Opens, updates or closes the outage issue.
    /// </summary>
    /// <param name="report">The report.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the issue is up to date.</returns>
    /// <exception cref="InvalidOperationException">GitHub did not accept a request.</exception>
    public async Task Report(ProbeReport report, CancellationToken cancellationToken)
    {
        await Send(HttpMethod.Post, $"repos/{repository}/labels", new JsonObject { ["name"] = Label }, cancellationToken, tolerate: 422);
        var issues = await Send(HttpMethod.Get, $"repos/{repository}/issues?labels={Label}&state=open&per_page=1", null, cancellationToken) as JsonArray ?? [];
        var number = issues.FirstOrDefault()?["number"]?.GetValue<long>();
        if (report.IsDown)
        {
            var body = new JsonObject { ["body"] = report.FailingText };
            if (number is { } open)
            {
                await Send(HttpMethod.Post, $"repos/{repository}/issues/{open}/comments", body, cancellationToken);
                return;
            }

            var title = $"Public endpoint probe failing: {string.Join(", ", report.Failing.Select(result => $"{result.Target}/{result.Check}"))}";
            await Send(HttpMethod.Post, $"repos/{repository}/issues", new JsonObject { ["title"] = title.Length > 240 ? title[..240] : title, ["body"] = report.FailingText, ["labels"] = new JsonArray(Label) }, cancellationToken);
            return;
        }

        if (number is { } recovered)
        {
            await Send(HttpMethod.Post, $"repos/{repository}/issues/{recovered}/comments", new JsonObject { ["body"] = "recovered" }, cancellationToken);
            await Send(HttpMethod.Patch, $"repos/{repository}/issues/{recovered}", new JsonObject { ["state"] = "closed" }, cancellationToken);
        }
    }

    async Task<JsonNode?> Send(HttpMethod method, string path, JsonNode? body, CancellationToken cancellationToken, int tolerate = 0)
    {
        using var request = new HttpRequestMessage(method, new Uri(_api, path));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.UserAgent.ParseAdd("cratis-synthetic-probe");
        request.Headers.Accept.ParseAdd("application/vnd.github+json");
        if (body is not null)
        {
            request.Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json");
        }

        using var response = await http.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return (int)response.StatusCode == tolerate ? null : throw new InvalidOperationException("GitHub issue delivery failed");
        }

        var text = await response.Content.ReadAsStringAsync(cancellationToken);
        return string.IsNullOrWhiteSpace(text) ? null : JsonNode.Parse(text);
    }
}
