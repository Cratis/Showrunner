// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Cryptography;
using System.Text;

namespace Cratis.Showrunner.BoxOffice.Offering.Declaration;

/// <summary>
/// Compares declarations by what they say, because <see cref="OfferingDeclared"/> holds collections that records
/// compare by reference.
/// </summary>
public static class OfferingDeclarations
{
    /// <summary>
    /// Gets whether two declarations say the same thing, whatever order they list their modules and meters in.
    /// </summary>
    /// <param name="first">The first declaration.</param>
    /// <param name="second">The second declaration.</param>
    /// <returns><see langword="true"/> when they are the same.</returns>
    public static bool AreSame(OfferingDeclared first, OfferingDeclared second) =>
        string.Equals(Canonical(first), Canonical(second), StringComparison.Ordinal);

    /// <summary>
    /// Gets a short stable identity of what a declaration says.
    /// </summary>
    /// <param name="declaration">The declaration.</param>
    /// <returns>A lowercase hexadecimal fingerprint.</returns>
    public static string Fingerprint(OfferingDeclared declaration) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(Canonical(declaration))))[..32];

    static string Canonical(OfferingDeclared declaration)
    {
        var modules = declaration.Modules
            .OrderBy(_ => _.Id.Value, StringComparer.Ordinal)
            .Select(_ => $"m:{_.Id.Value.Length}:{_.Id.Value}:{_.Name.Value.Length}:{_.Name.Value}");
        var meters = declaration.Meters
            .OrderBy(_ => _.Id.Value, StringComparer.Ordinal)
            .Select(_ => $"t:{_.Id.Value.Length}:{_.Id.Value}:{_.Name.Value.Length}:{_.Name.Value}:{_.Unit.Value.Length}:{_.Unit.Value}");
        return string.Join('|', modules.Concat(meters).Append($"s:{declaration.Seats}"));
    }
}
