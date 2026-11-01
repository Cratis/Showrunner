// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.StageOperator.Configuration;

namespace Cratis.Showrunner.StageOperator.Hosting;

/// <summary>Resolves which namespaces the operator watches.</summary>
public static class WatchScope
{
    /// <summary>Gets the namespaces to list and watch; a single null entry means every namespace.</summary>
    /// <param name="options">The operator configuration.</param>
    /// <returns>The namespaces.</returns>
    public static IReadOnlyList<string?> Namespaces(StageOperatorOptions options)
    {
        var configured = options.WatchNamespaces.Where(n => !string.IsNullOrWhiteSpace(n)).Select(n => n.Trim()).Distinct(StringComparer.Ordinal).ToList();
        return configured.Count == 0 ? [null] : [.. configured];
    }
}
