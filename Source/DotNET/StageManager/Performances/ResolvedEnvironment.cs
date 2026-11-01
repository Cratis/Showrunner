// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// The values of a Cratis environment after its secrets were resolved. Keep it in memory: it is handed to containers as mounted files and goes nowhere else.
/// </summary>
/// <param name="Name">The name of the environment.</param>
/// <param name="Values">The resolved values by key.</param>
public record ResolvedEnvironment(string Name, IReadOnlyDictionary<string, string> Values)
{
    /// <summary>
    /// Gets the keys among <paramref name="keys"/> that have no non-empty value.
    /// </summary>
    /// <param name="keys">The keys that are needed.</param>
    /// <returns>The missing keys, in the order given.</returns>
    public IReadOnlyList<string> Missing(IEnumerable<string> keys) =>
        [.. keys.Where(key => !Values.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))];

    /// <summary>
    /// Gets the values whose key starts with a prefix.
    /// </summary>
    /// <param name="prefix">The prefix.</param>
    /// <returns>The matching keys and values.</returns>
    public IEnumerable<KeyValuePair<string, string>> WithPrefix(string prefix) =>
        Values.Where(pair => pair.Key.StartsWith(prefix, StringComparison.Ordinal));
}
