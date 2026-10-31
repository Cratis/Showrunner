// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Offering.Declaration;

/// <summary>
/// Thrown when a product declares an offering that cannot be sold, so the product does not start with it.
/// </summary>
/// <param name="problems">What is wrong with the declaration.</param>
public class InvalidOffering(IEnumerable<string> problems)
    : Exception($"The product's offering is not valid: {string.Join(' ', problems)}")
{
    /// <summary>
    /// Gets what is wrong with the declaration.
    /// </summary>
    public IReadOnlyList<string> Problems { get; } = [.. problems];
}
