// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Cratis.Showrunner.BoxOffice.Offering.Declaration;

namespace Cratis.Showrunner.BoxOffice.Offering;

/// <summary>
/// Extension methods for addressing what the product reports to its operator.
/// </summary>
public static class OperatorAudiences
{
    /// <summary>
    /// Addresses everything appended to the outbox inside the returned scope to the operator's audience.
    /// </summary>
    /// <param name="options">The <see cref="OfferingOptions"/>, or <see langword="null" /> when the code has none.</param>
    /// <returns>
    /// The scope to dispose when the append is done, or <see langword="null" /> when no operator audience is configured, in which case
    /// nothing is addressed and the outbox behaves as it did before audiences.
    /// </returns>
    public static IDisposable? AddressedToOperator(this OfferingOptions? options) =>
        string.IsNullOrWhiteSpace(options?.OperatorAudience) ? null : EventTags.Audience(options.OperatorAudience);
}
