// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.BoxOffice.Offering.Declaration;

namespace Cratis.Showrunner.BoxOffice.Offering.Metering;

/// <summary>
/// Finds what keeps the registered meters from measuring the declared ones.
/// </summary>
public static class MeteringProblems
{
    /// <summary>
    /// Finds every problem with the registered meters.
    /// </summary>
    /// <param name="offering">The product's offering.</param>
    /// <param name="meters">The registered meters.</param>
    /// <returns>A sentence for each problem; none when the meters can be measured.</returns>
    public static IEnumerable<string> In(ProductOffering offering, IEnumerable<IMeter> meters)
    {
        var declared = offering.Meters.Select(_ => _.Id).ToHashSet();
        var registered = meters.ToArray();

        foreach (var meter in registered.Where(_ => !declared.Contains(_.Id)))
        {
            yield return $"Meter '{meter.Id}' is not declared in the offering: declare it with Meter(...) so it can be sold.";
        }

        foreach (var duplicate in registered.GroupBy(_ => _.Id).Where(_ => _.Count() > 1))
        {
            yield return $"Meter '{duplicate.Key}' is measured by {duplicate.Count()} registered meters.";
        }

        foreach (var meter in registered.Where(_ => _.EventTypes.Count == 0))
        {
            yield return $"Meter '{meter.Id}' reads no events.";
        }

        foreach (var meter in offering.Meters.Where(_ => _.Id.Value.Contains('.', StringComparison.Ordinal) || _.Id.Value.StartsWith('$')))
        {
            yield return $"Meter '{meter.Id}' cannot be kept: a key may not contain '.' or start with '$'.";
        }
    }
}
