// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Showrunner.Operations.Probe;

/// <summary>
/// Reads the probe targets from JSON, in the shape the Cratis probe has always used: an array of targets, each with a name, a base
/// address and its checks (<c>id</c>, <c>method</c>, <c>path</c>, <c>accept</c>, <c>expectStatus</c>, and optionally <c>body</c> and <c>timeoutMs</c>).
/// </summary>
public static class ProbeTargets
{
    static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Reads the targets.
    /// </summary>
    /// <param name="json">The JSON.</param>
    /// <returns>The targets.</returns>
    /// <exception cref="InvalidOperationException">The JSON is not a list of valid targets.</exception>
    public static IReadOnlyList<ProbeTarget> Parse(string json)
    {
        List<TargetJson>? targets;
        try
        {
            targets = JsonSerializer.Deserialize<List<TargetJson>>(json, _options);
        }
        catch (JsonException)
        {
            throw new InvalidOperationException("the probe targets are not valid JSON");
        }

        return targets is null || targets.Count == 0
            ? throw new InvalidOperationException("the probe targets are empty")
            : [.. targets.Select(Map)];
    }

    /// <summary>
    /// Reads the base address overrides, a JSON object from target name to address.
    /// </summary>
    /// <param name="json">The JSON, or <see langword="null"/> for none.</param>
    /// <returns>The overrides.</returns>
    /// <exception cref="InvalidOperationException">The JSON is not an object of strings.</exception>
    public static IReadOnlyDictionary<string, string> ParseOverrides(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new Dictionary<string, string>();
        }

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json, _options) ?? [];
        }
        catch (JsonException)
        {
            throw new InvalidOperationException("the probe base addresses are not a JSON object of strings");
        }
    }

    static ProbeTarget Map(TargetJson target)
    {
        if (string.IsNullOrWhiteSpace(target.Name) || !Uri.TryCreate(target.BaseUrl, UriKind.Absolute, out var url) || (url.Scheme != Uri.UriSchemeHttp && url.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException("a probe target needs a name and an http or https base address");
        }

        if (target.Checks is not { Count: > 0 })
        {
            throw new InvalidOperationException($"the probe target {target.Name} has no checks");
        }

        return new ProbeTarget(target.Name, target.BaseUrl, [.. target.Checks.Select(check => Map(target.Name, check))], target.Note);
    }

    static ProbeEndpoint Map(string target, CheckJson check) =>
        string.IsNullOrWhiteSpace(check.Id) || string.IsNullOrWhiteSpace(check.Path) || !check.Path.StartsWith('/') || check.ExpectStatus is null
            ? throw new InvalidOperationException($"a check of the probe target {target} needs an id, a path beginning with a slash and an expected status")
            : new ProbeEndpoint(check.Id, check.Method ?? "GET", check.Path, check.Accept ?? "application/json", check.ExpectStatus.Value, check.Body, check.TimeoutMs);

    sealed record TargetJson(string? Name, string? BaseUrl, List<CheckJson>? Checks, string? Note);

    sealed record CheckJson(string? Id, string? Method, string? Path, string? Accept, int? ExpectStatus, string? Body, int? TimeoutMs);
}
