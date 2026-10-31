// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Offering.Metering;

/// <summary>
/// The metering buckets of one tenant for one UTC day: how much of each meter the tenant used.
/// </summary>
/// <param name="Tenant">The tenant.</param>
/// <param name="Day">The UTC day.</param>
/// <param name="Organization">The organization the day is reported under.</param>
/// <param name="IsClosed">Whether the day has been reported as closed.</param>
/// <param name="CorrectionDue">The sequence number of the latest event that arrived after the day closed and is not yet reported, or <see langword="null"/> when nothing waits to be corrected.</param>
/// <param name="Quantities">The total of each meter that has a bucket.</param>
public record MeteringDay(
    TenantId Tenant,
    ComputeDay Day,
    OrganizationId Organization,
    bool IsClosed,
    EventSequenceNumber? CorrectionDue,
    IReadOnlyDictionary<MeterId, MeterQuantity> Quantities)
{
    /// <summary>
    /// Gets the totals as reported in the contract events, in a fixed order.
    /// </summary>
    public IEnumerable<MeterUsage> Usage => Quantities
        .OrderBy(_ => _.Key.Value, StringComparer.Ordinal)
        .Select(_ => new MeterUsage(_.Key, _.Value));
}
