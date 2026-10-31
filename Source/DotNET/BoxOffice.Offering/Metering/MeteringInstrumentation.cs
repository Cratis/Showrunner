// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Diagnostics;
using Cratis.Metrics;

namespace Cratis.Showrunner.BoxOffice.Offering.Metering;

/// <summary>
/// The metrics of measuring compute.
/// </summary>
public static partial class MeteringInstrumentation
{
    [Counter<int>("boxoffice.metering.buckets", "Metering buckets updated with a quantity")]
    internal static partial void BucketUpdated(this IMeter<TenantMetering> meter, [Tag("boxoffice.meter")] string meterId);

    [Counter<int>("boxoffice.metering.days.closed", "Days reported as closed")]
    internal static partial void DayClosed(this IMeter<TenantMetering> meter);

    [Counter<int>("boxoffice.metering.corrections", "Corrections reported for a day that was already closed")]
    internal static partial void DayCorrected(this IMeter<TenantMetering> meter);
}
