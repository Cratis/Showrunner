// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.BoxOffice.Product.Activity;

internal static partial class ActivityReporterLogging
{
    [LoggerMessage(LogLevel.Debug, "Activity in tenant '{Tenant}' was not reported: the product has no such tenant")]
    internal static partial void UnknownTenant(this ILogger<ActivityReporter> logger, TenantId tenant);

    [LoggerMessage(LogLevel.Warning, "Activity in tenant '{Tenant}' was not reported: the tenant has not been told which organization it is yet")]
    internal static partial void OrganizationNotKnown(this ILogger<ActivityReporter> logger, TenantId tenant);
}
