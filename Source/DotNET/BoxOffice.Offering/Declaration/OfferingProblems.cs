// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Offering.Declaration;

/// <summary>
/// Finds what keeps an offering from being sold.
/// </summary>
public static class OfferingProblems
{
    /// <summary>
    /// Finds every problem with an offering.
    /// </summary>
    /// <param name="offering">The offering.</param>
    /// <param name="hasProduct">Whether the product's identity was set.</param>
    /// <returns>A sentence for each problem; none when the offering is valid.</returns>
    public static IEnumerable<string> In(ProductOffering offering, bool hasProduct)
    {
        if (!hasProduct)
        {
            yield return "The product has no identity: call Product(...) with the id the operator knows it by.";
        }

        foreach (var problem in InModules(offering).Concat(InMeters(offering)).Concat(InSeats(offering)))
        {
            yield return problem;
        }
    }

    static IEnumerable<string> InModules(ProductOffering offering)
    {
        var ids = offering.Modules.Select(_ => _.Id).ToHashSet();

        foreach (var duplicate in offering.Modules.GroupBy(_ => _.Id).Where(_ => _.Count() > 1))
        {
            yield return $"Module '{duplicate.Key}' is declared {duplicate.Count()} times.";
        }

        foreach (var module in offering.Modules)
        {
            if (string.IsNullOrWhiteSpace(module.Id.Value))
            {
                yield return "A module has no key.";
            }

            if (string.IsNullOrWhiteSpace(module.Name.Value))
            {
                yield return $"Module '{module.Id}' has no name.";
            }

            if (module.DefaultTrialDays < 0)
            {
                yield return $"Module '{module.Id}' has a trial of {module.DefaultTrialDays} days; a trial cannot be negative.";
            }

            foreach (var required in module.Requires.Where(_ => !ids.Contains(_)))
            {
                yield return $"Module '{module.Id}' requires module '{required}', which the product does not declare.";
            }

            if (module.Requires.Contains(module.Id))
            {
                yield return $"Module '{module.Id}' requires itself.";
            }

            foreach (var @namespace in module.Namespaces.Where(string.IsNullOrWhiteSpace))
            {
                yield return $"Module '{module.Id}' owns an empty namespace: {@namespace ?? "null"}.";
            }
        }

        foreach (var shared in offering.Modules
            .SelectMany(module => module.Namespaces.Distinct(StringComparer.Ordinal).Select(@namespace => (module.Id, Namespace: @namespace)))
            .GroupBy(_ => _.Namespace, StringComparer.Ordinal)
            .Where(_ => _.Count() > 1))
        {
            yield return $"Namespace '{shared.Key}' is owned by more than one module: {string.Join(", ", shared.Select(_ => $"'{_.Id}'"))}.";
        }

        foreach (var cycle in RequirementCycles(offering))
        {
            yield return $"Modules require each other in a circle: {cycle}.";
        }
    }

    static IEnumerable<string> RequirementCycles(ProductOffering offering)
    {
        var requires = offering.Modules
            .GroupBy(_ => _.Id)
            .ToDictionary(_ => _.Key, _ => _.First().Requires.Where(r => r != _.Key).ToArray());
        var reported = new HashSet<string>(StringComparer.Ordinal);

        foreach (var start in requires.Keys.OrderBy(_ => _.Value, StringComparer.Ordinal))
        {
            var path = new List<ModuleId>();
            var cycle = Walk(start, requires, path);
            if (cycle is not null)
            {
                var members = cycle.Take(cycle.Count - 1).OrderBy(_ => _.Value, StringComparer.Ordinal).Select(_ => _.Value);
                if (reported.Add(string.Join('|', members)))
                {
                    yield return string.Join(" -> ", cycle.Select(_ => $"'{_}'"));
                }
            }
        }
    }

    static List<ModuleId>? Walk(ModuleId current, Dictionary<ModuleId, ModuleId[]> requires, List<ModuleId> path)
    {
        var at = path.IndexOf(current);
        if (at >= 0)
        {
            return [.. path.Skip(at), current];
        }

        if (!requires.TryGetValue(current, out var next))
        {
            return null;
        }

        path.Add(current);
        foreach (var required in next)
        {
            var cycle = Walk(required, requires, path);
            if (cycle is not null)
            {
                return cycle;
            }
        }

        path.RemoveAt(path.Count - 1);
        return null;
    }

    static IEnumerable<string> InMeters(ProductOffering offering)
    {
        foreach (var duplicate in offering.Meters.GroupBy(_ => _.Id).Where(_ => _.Count() > 1))
        {
            yield return $"Meter '{duplicate.Key}' is declared {duplicate.Count()} times.";
        }

        foreach (var meter in offering.Meters)
        {
            if (string.IsNullOrWhiteSpace(meter.Id.Value))
            {
                yield return "A meter has no key.";
            }

            if (string.IsNullOrWhiteSpace(meter.Name.Value))
            {
                yield return $"Meter '{meter.Id}' has no name.";
            }

            if (string.IsNullOrWhiteSpace(meter.Unit.Value))
            {
                yield return $"Meter '{meter.Id}' counts in no unit.";
            }
        }
    }

    static IEnumerable<string> InSeats(ProductOffering offering)
    {
        if (offering.Seats.Licensing == SeatLicensing.NotLicensed && offering.Seats.Roles.Any())
        {
            yield return "Seats name roles that need one, but the product does not license seats.";
        }
    }
}
