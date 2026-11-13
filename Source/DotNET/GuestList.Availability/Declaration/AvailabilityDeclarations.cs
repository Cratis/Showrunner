// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Cratis.Showrunner.GuestList.Contracts.Audiences;
using Cratis.Showrunner.GuestList.Contracts.Availability;
using Manifest = Cratis.Choreography.Availability;

namespace Cratis.Showrunner.GuestList.Availability.Declaration;

/// <summary>
/// Turns the availability manifest of a product into what it reports, and compares reports by what they say.
/// </summary>
public static class AvailabilityDeclarations
{
    /// <summary>
    /// Describes the manifest as the product reports it. Everything is put in order, so declaring the same things in
    /// another order is the same declaration.
    /// </summary>
    /// <param name="manifest">The manifest.</param>
    /// <param name="product">The product.</param>
    /// <param name="environment">The environment the product reports from.</param>
    /// <param name="version">The version of the product build.</param>
    /// <param name="everyDeclaredAudienceGranted">Whether the environment grants every declared audience to everyone.</param>
    /// <returns>The declaration.</returns>
    public static AvailabilityDeclared From(Manifest.AvailabilityManifest manifest, Product product, EnvironmentName environment, ModelVersion version, bool everyDeclaredAudienceGranted) =>
        new(
            product,
            environment,
            version,
            manifest.Origin,
            everyDeclaredAudienceGranted,
            [.. manifest.Audiences.Select(audience => new AudienceDeclaration(audience.Name.Value, audience.Description, audience.Source)).OrderBy(audience => audience.Name.Value, StringComparer.Ordinal)],
            [.. manifest.Blocks.Select(block => new AvailabilityBlockDeclaration(block.Name, block.Description, block.Source, Alternatives(block.Alternatives))).OrderBy(block => block.Name, StringComparer.Ordinal)],
            [.. manifest.Gates.Select(gate => new GateDeclaration(gate.Target, Alternatives(gate.Alternatives))).OrderBy(gate => gate.Target, StringComparer.Ordinal)],
            [.. manifest.ContextValues.Select(value => new ContextValueDeclaration(value.Name, value.Type, value.Origin, string.Join('.', value.Scope.Path), value.Required)).OrderBy(value => value.Name, StringComparer.Ordinal)],
            [.. manifest.Personas.Select(persona => new PersonaDeclaration(persona.Name, Names(persona.Audiences.Select(audience => audience.Value)))).OrderBy(persona => persona.Name, StringComparer.Ordinal)]);

    /// <summary>
    /// Gets whether two declarations say the same. The version of the build is not compared: a new build that declares the same
    /// is not a change, so it is not reported.
    /// </summary>
    /// <param name="first">The first declaration.</param>
    /// <param name="second">The second declaration.</param>
    /// <returns><see langword="true"/> when they declare the same.</returns>
    public static bool AreSame(AvailabilityDeclared first, AvailabilityDeclared second) =>
        string.Equals(Canonical(first), Canonical(second), StringComparison.Ordinal);

    /// <summary>
    /// Gets a fingerprint of what a declaration says, to key the report of it.
    /// </summary>
    /// <param name="declaration">The declaration.</param>
    /// <returns>The fingerprint.</returns>
    public static string Fingerprint(AvailabilityDeclared declaration) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(Canonical(declaration))));

    static string Canonical(AvailabilityDeclared declaration) =>
        JsonSerializer.Serialize(declaration with { Version = ModelVersion.NotSet });

    static IEnumerable<AvailabilityAlternative> Alternatives(IEnumerable<Manifest.AvailabilityAlternative> alternatives) =>
    [
        .. alternatives
            .Select(alternative => new AvailabilityAlternative(Names(alternative.Audiences.Select(audience => audience.Value)), [.. alternative.Conditions.Order(StringComparer.Ordinal)]))
            .OrderBy(alternative => string.Join('\n', alternative.Audiences.Select(audience => audience.Value).Concat(alternative.Conditions)), StringComparer.Ordinal)
    ];

    static IEnumerable<AudienceName> Names(IEnumerable<string> names) =>
        [.. names.Order(StringComparer.Ordinal).Select(name => new AudienceName(name))];
}
