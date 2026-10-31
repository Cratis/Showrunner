// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.BoxOffice.Compute;

internal static partial class TenantComputeAdmissionLogging
{
    [LoggerMessage(LogLevel.Information, "Turned away work of meter '{Meter}' for tenant '{Tenant}': {Outcome}")]
    internal static partial void TurnedAway(this ILogger<TenantComputeAdmission> logger, MeterId meter, TenantId tenant, AdmissionOutcome outcome);

    [LoggerMessage(LogLevel.Information, "Parked work of meter '{Meter}' for tenant '{Tenant}' until {Until}: {Because}")]
    internal static partial void Parked(this ILogger<TenantComputeAdmission> logger, MeterId meter, TenantId tenant, DateTimeOffset until, AdmissionOutcome because);

    [LoggerMessage(LogLevel.Warning, "Work of meter '{Meter}' for tenant '{Tenant}' was refused as at its concurrency limit because other work kept starting while this was decided")]
    internal static partial void Contended(this ILogger<TenantComputeAdmission> logger, MeterId meter, TenantId tenant);
}
