// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.BoxOffice.Product.Provisioning;

internal static partial class TenantRegistrationsLogging
{
    [LoggerMessage(LogLevel.Information, "Tenant '{Tenant}' was provisioned; it is reported ready")]
    internal static partial void Provisioned(this ILogger<TenantRegistrations> logger, TenantId tenant);

    [LoggerMessage(LogLevel.Warning, "The product refused to provision tenant '{Tenant}': {Reason}")]
    internal static partial void Refused(this ILogger<TenantRegistrations> logger, TenantId tenant, NotProvisionedReason reason);

    [LoggerMessage(LogLevel.Warning, "Tenant '{Tenant}' was reported ready or refused with a different answer before; the first answer stands")]
    internal static partial void AnsweredDifferently(this ILogger<TenantRegistrations> logger, TenantId tenant);

    [LoggerMessage(LogLevel.Information, "Tenant '{Tenant}' is closing; the product was told")]
    internal static partial void Closing(this ILogger<TenantRegistrations> logger, TenantId tenant);

    [LoggerMessage(LogLevel.Debug, "Tenant '{Tenant}' is closing and the product does not take part in tearing tenants down")]
    internal static partial void ClosingIgnored(this ILogger<TenantRegistrations> logger, TenantId tenant);
}
