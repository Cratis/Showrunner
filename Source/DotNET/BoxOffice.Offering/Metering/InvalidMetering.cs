// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Offering.Metering;

/// <summary>
/// Exception that gets thrown when the product's meters do not match what it declares, so the product does not start.
/// </summary>
/// <param name="problems">What is wrong, one sentence each.</param>
public class InvalidMetering(IEnumerable<string> problems)
    : Exception($"The product's meters cannot be measured: {string.Join(' ', problems)}")
{
    /// <summary>
    /// Gets what is wrong, one sentence each.
    /// </summary>
    public IReadOnlyList<string> Problems { get; } = [.. problems];
}
