// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Diagnostics;
using Cratis.Metrics;

namespace Cratis.Showrunner.BoxOffice.Compute;

/// <summary>
/// The metrics of compute admission.
/// </summary>
public static partial class ComputeInstrumentation
{
    [Counter<int>("boxoffice.compute.admitted", "Units of work admitted")]
    internal static partial void Admitted(this IMeter<TenantComputeAdmission> meter, [Tag("boxoffice.meter")] string meterId);

    [Counter<int>("boxoffice.compute.refused", "Units of work refused or parked, by what turned them away")]
    internal static partial void Refused(this IMeter<TenantComputeAdmission> meter, [Tag("boxoffice.meter")] string meterId, [Tag("boxoffice.outcome")] string outcome);
}
