// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Cratis.Showrunner.GuestList.Contracts.Availability;

namespace Cratis.Showrunner.GuestList.Authoring.Availability;

/// <summary>
/// What one report of a product's declaration changed in an environment, compared with the one before it.
/// </summary>
/// <param name="Environment">The environment the product reported from.</param>
/// <param name="Version">The version of the product build that reported.</param>
/// <param name="At">When it was reported.</param>
/// <param name="Added">What the model declares now and did not before. The first report adds everything it declares.</param>
/// <param name="Removed">What the model declared before and does not now.</param>
/// <param name="Changed">What the model declares both before and now, but differently.</param>
public record ProductAvailabilityChange(
    EnvironmentName Environment,
    ModelVersion Version,
    DateTimeOffset At,
    IEnumerable<DeclaredItem> Added,
    IEnumerable<DeclaredItem> Removed,
    IEnumerable<DeclaredItem> Changed)
{
    /// <summary>
    /// Compares a declaration with the one before it.
    /// </summary>
    /// <param name="previous">The declaration before, or <see langword="null"/> for the first report of the environment.</param>
    /// <param name="declared">The declaration now.</param>
    /// <returns>What changed.</returns>
    public static ProductAvailabilityChange Between(EnvironmentAvailability? previous, EnvironmentAvailability declared)
    {
        var before = previous is null ? [] : Describe(previous);
        var after = Describe(declared);
        return new(
            declared.Environment,
            declared.Version,
            declared.DeclaredAt,
            Ordered(after.Keys.Where(item => !before.ContainsKey(item))),
            Ordered(before.Keys.Where(item => !after.ContainsKey(item))),
            Ordered(after.Keys.Where(item => before.TryGetValue(item, out var text) && text != after[item])));
    }

    static Dictionary<DeclaredItem, string> Describe(EnvironmentAvailability declaration)
    {
        var items = new Dictionary<DeclaredItem, string>();
        foreach (var audience in declaration.Audiences)
        {
            items[new(DeclaredItemKind.Audience, audience.Name.Value)] = Text(audience);
        }

        foreach (var block in declaration.Availabilities)
        {
            items[new(DeclaredItemKind.Availability, block.Name)] = Text(block);
        }

        foreach (var gate in declaration.Gates)
        {
            items[new(DeclaredItemKind.Gate, gate.Target)] = Text(gate);
        }

        foreach (var value in declaration.ContextValues)
        {
            items[new(DeclaredItemKind.ContextValue, value.Name)] = Text(value);
        }

        foreach (var persona in declaration.Personas)
        {
            items[new(DeclaredItemKind.Persona, persona.Name)] = Text(persona);
        }

        return items;
    }

    static string Text<T>(T item) => JsonSerializer.Serialize(item);

    static IReadOnlyList<DeclaredItem> Ordered(IEnumerable<DeclaredItem> items) =>
        [.. items.OrderBy(item => item.Kind).ThenBy(item => item.Name, StringComparer.Ordinal)];
}
