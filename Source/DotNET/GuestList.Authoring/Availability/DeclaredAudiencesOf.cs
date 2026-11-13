// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.GuestList.Authoring.Availability;

/// <summary>
/// Decides which audiences a product declares, as far as the operator knows.
/// </summary>
public static class DeclaredAudiencesOf
{
    /// <summary>
    /// Gets the audiences a product declares: what the product reported for the primary environment, which beats what is
    /// configured, or else what is configured.
    /// </summary>
    /// <param name="product">The product.</param>
    /// <param name="availabilities">What the product reported.</param>
    /// <param name="options">The <see cref="GuestListAuthoringOptions"/>.</param>
    /// <returns>The audiences, or <see langword="null"/> when nothing is known about the product's declaration.</returns>
    public static IEnumerable<AudienceName>? Product(Product product, IEnumerable<ProductAvailability> availabilities, GuestListAuthoringOptions options) =>
        availabilities.Select(availability => availability.DeclaredAudiences(options.PrimaryEnvironment)).FirstOrDefault(declared => declared is not null)
        ?? (options.DeclaredAudiences.TryGetValue(product, out var configured) ? configured : null);
}
