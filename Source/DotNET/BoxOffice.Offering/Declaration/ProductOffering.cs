// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Offering.Declaration;

/// <summary>
/// What a product sells: its modules, its meters and how it licenses seats. Built and checked by
/// <see cref="ProductOfferingBuilder"/>, so one that exists is valid.
/// </summary>
/// <param name="Product">The product's identity, the event source its declaration is reported under.</param>
/// <param name="Modules">The product's modules.</param>
/// <param name="Meters">The product's meters.</param>
/// <param name="Seats">How the product licenses seats.</param>
public record ProductOffering(
    ProductId Product,
    IReadOnlyList<ModuleDefinition> Modules,
    IReadOnlyList<MeterDefinition> Meters,
    SeatPolicy Seats)
{
    /// <summary>
    /// Finds the module that owns a code namespace: the one that owns the longest namespace the given one is, or is inside of.
    /// </summary>
    /// <param name="namespace">The namespace, such as <c>Acme.Analytics.Reports</c>.</param>
    /// <returns>The module, or <see langword="null"/> when no module owns the namespace.</returns>
    public ModuleDefinition? ModuleOwning(string @namespace) =>
        Modules
            .SelectMany(module => module.Namespaces.Where(owned => Owns(owned, @namespace)).Select(owned => (module, owned.Length)))
            .OrderByDescending(match => match.Length)
            .Select(match => match.module)
            .FirstOrDefault();

    /// <summary>
    /// Gets the declaration to report to the operator. Modules and meters are in a fixed order so the same offering is
    /// always the same declaration.
    /// </summary>
    /// <returns>The <see cref="OfferingDeclared"/> event.</returns>
    public OfferingDeclared ToDeclaration() => new(
        [.. Modules.OrderBy(_ => _.Id.Value, StringComparer.Ordinal).Select(_ => new ModuleDeclaration(_.Id, _.Name))],
        [.. Meters.OrderBy(_ => _.Id.Value, StringComparer.Ordinal).Select(_ => new MeterDeclaration(_.Id, _.Name, _.Unit))],
        Seats.Licensing);

    static bool Owns(string owned, string @namespace) =>
        string.Equals(owned, @namespace, StringComparison.Ordinal)
        || @namespace.StartsWith(owned + ".", StringComparison.Ordinal);
}
