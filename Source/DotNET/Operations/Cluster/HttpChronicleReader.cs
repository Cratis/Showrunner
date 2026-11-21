// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Cratis.Showrunner.Operations.Cluster;

/// <summary>
/// Reads Chronicle over its HTTP query API with a client-credentials token. The token is fetched once and reused. The client secret
/// is only ever sent to the token endpoint and is never part of an error.
/// </summary>
/// <param name="http">The HTTP client; it trusts Chronicle's certificate authority when Chronicle uses an internal one.</param>
/// <param name="baseUri">The Chronicle API address.</param>
/// <param name="clientId">The client id.</param>
/// <param name="clientSecret">The client secret.</param>
public sealed class HttpChronicleReader(HttpClient http, Uri baseUri, string clientId, string clientSecret) : IChronicleReader
{
    const string SystemStore = "System";

    readonly Lock _gate = new();
    Task<string>? _token;

    /// <inheritdoc/>
    public async Task<IReadOnlyList<string>> EventStores(TimeSpan timeout, CancellationToken cancellationToken)
    {
        // The System store is Chronicle's own and reserved for administrators; the watchdog's application client is refused it
        // by design, so listing it would raise a finding that can never clear.
        var stores = await Names("/api/event-stores/all-event-stores", [], timeout, cancellationToken);
        return [.. stores.Where(store => !string.Equals(store, SystemStore, StringComparison.Ordinal))];
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<string>> Namespaces(string eventStore, TimeSpan timeout, CancellationToken cancellationToken) =>
        Names("/api/namespaces/all-namespaces", [("eventStore", eventStore)], timeout, cancellationToken);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ChronicleFailedPartition>> FailedPartitions(string eventStore, string @namespace, TimeSpan timeout, CancellationToken cancellationToken)
    {
        var data = await Query("/api/observation/get-failed-partitions", [("eventStore", eventStore), ("namespace", @namespace)], timeout, cancellationToken);
        return [.. data.Select(partition => new ChronicleFailedPartition(
            Text(partition, "observerId"),
            Flag(partition, "isResolved"),
            Flag(partition, "isQuarantined"),
            [.. Attempts(partition)]))];
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ChronicleObserver>> Observers(string eventStore, string @namespace, TimeSpan timeout, CancellationToken cancellationToken)
    {
        var data = await Query("/api/observation/all-observers", [("eventStore", eventStore), ("namespace", @namespace)], timeout, cancellationToken);
        return [.. data.Select(observer => new ChronicleObserver(
            Text(observer, "id"),
            observer.TryGetProperty("runningState", out var state) && state.ValueKind == JsonValueKind.Number ? state.GetInt32() : -1))];
    }

    static string Text(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()!
            : throw new InvalidOperationException($"a Chronicle answer has no {name}");

    static bool Flag(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.True;

    static IEnumerable<DateTimeOffset> Attempts(JsonElement partition)
    {
        if (!partition.TryGetProperty("attempts", out var attempts) || attempts.ValueKind != JsonValueKind.Array)
        {
            yield break;
        }

        foreach (var attempt in attempts.EnumerateArray())
        {
            yield return DateTimeOffset.Parse(Text(attempt, "occurred"), CultureInfo.InvariantCulture);
        }
    }

    static string Reasons(JsonElement root)
    {
        var reasons = new List<string>();
        if (root.TryGetProperty("validationResults", out var results) && results.ValueKind == JsonValueKind.Array)
        {
            reasons.AddRange(results.EnumerateArray().Select(result => result.TryGetProperty("message", out var message) ? message.GetString() ?? string.Empty : string.Empty));
        }

        if (root.TryGetProperty("exceptionMessages", out var exceptions) && exceptions.ValueKind == JsonValueKind.Array)
        {
            reasons.AddRange(exceptions.EnumerateArray().Select(message => message.GetString() ?? string.Empty));
        }

        var text = string.Join("; ", reasons.Where(reason => reason.Length > 0));
        return text.Length > 0 ? text : "query did not succeed";
    }

    async Task<IReadOnlyList<string>> Names(string path, (string Name, string Value)[] parameters, TimeSpan timeout, CancellationToken cancellationToken)
    {
        var data = await Query(path, parameters, timeout, cancellationToken);
        return [.. data.Select(item => Text(item, "name"))];
    }

    async Task<string> Token(TimeSpan timeout, CancellationToken cancellationToken)
    {
        Task<string> pending;
        lock (_gate)
        {
            pending = _token ??= FetchToken(timeout, cancellationToken);
        }

        try
        {
            return await pending;
        }
        catch
        {
            // A failed token is never kept: the next read asks again.
            lock (_gate)
            {
                if (ReferenceEquals(_token, pending))
                {
                    _token = null;
                }
            }

            throw;
        }
    }

    async Task<string> FetchToken(TimeSpan timeout, CancellationToken cancellationToken)
    {
        using var limit = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        limit.CancelAfter(timeout);
        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(baseUri, "/connect/token"))
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret
            })
        };
        using var response = await http.SendAsync(request, limit.Token);
        response.EnsureSuccessStatusCode();
        using var body = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(limit.Token), cancellationToken: limit.Token);
        return body.RootElement.TryGetProperty("access_token", out var token) && token.ValueKind == JsonValueKind.String
            ? token.GetString()!
            : throw new InvalidOperationException("Chronicle's token answer has no access_token");
    }

    async Task<List<JsonElement>> Query(string path, (string Name, string Value)[] parameters, TimeSpan timeout, CancellationToken cancellationToken)
    {
        var token = await Token(timeout, cancellationToken);
        var query = string.Join('&', parameters.Select(parameter => $"{parameter.Name}={Uri.EscapeDataString(parameter.Value)}"));
        using var limit = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        limit.CancelAfter(timeout);
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(baseUri, $"{path}?{query}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var response = await http.SendAsync(request, limit.Token);
        response.EnsureSuccessStatusCode();
        using var body = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(limit.Token), cancellationToken: limit.Token);

        // Every query answers 200 with an envelope; one that did not succeed has no data and must not read as healthy.
        var root = body.RootElement;
        if (!root.TryGetProperty("isSuccess", out var success) || success.ValueKind != JsonValueKind.True)
        {
            throw new InvalidOperationException(Reasons(root));
        }

        if (!root.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return [.. data.EnumerateArray().Select(item => item.Clone())];
    }
}
