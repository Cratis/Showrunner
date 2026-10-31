// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.BoxOffice.Offering.Metering;

/// <summary>
/// Log messages for measuring compute.
/// </summary>
internal static partial class MeteringLogging
{
    [LoggerMessage(LogLevel.Information, "Reported the compute of tenant '{Tenant}' for {Day} as closed: {Meters} meter(s)")]
    internal static partial void DayClosed(this ILogger<TenantMetering> logger, TenantId tenant, ComputeDay day, int meters);

    [LoggerMessage(LogLevel.Information, "Reported a correction of the compute of tenant '{Tenant}' for {Day}, after the day closed")]
    internal static partial void DayCorrected(this ILogger<TenantMetering> logger, TenantId tenant, ComputeDay day);

    [LoggerMessage(LogLevel.Debug, "Compute of tenant '{Tenant}' for {Day} was already reported")]
    internal static partial void AlreadyReported(this ILogger<TenantMetering> logger, TenantId tenant, ComputeDay day);

    [LoggerMessage(LogLevel.Error, "Could not measure the compute of tenant '{Tenant}'; trying again soon")]
    internal static partial void MeasuringFailed(this ILogger<MeteringService> logger, TenantId tenant, Exception exception);

    [LoggerMessage(LogLevel.Warning, "Could not measure compute; trying again soon")]
    internal static partial void CycleFailed(this ILogger<MeteringService> logger, Exception exception);
}
