// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Cluster;

/// <summary>
/// One sample of an instant vector.
/// </summary>
/// <param name="Labels">The labels of the series.</param>
/// <param name="Value">The value.</param>
public sealed record PrometheusSample(IReadOnlyDictionary<string, string> Labels, double Value)
{
    /// <summary>
    /// Gets a label, or <see langword="null"/> when the series does not have it.
    /// </summary>
    /// <param name="name">The label name.</param>
    /// <returns>The value of the label.</returns>
    public string? Label(string name) => Labels.TryGetValue(name, out var value) ? value : null;
}
