// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.BoxOffice.Contracts.Offers;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.BoxOffice.Offering.Starting;

/// <summary>
/// Log messages for <see cref="DefaultOffering"/>.
/// </summary>
internal static partial class DefaultOfferingLogging
{
    [LoggerMessage(LogLevel.Information, "Started tenant '{Tenant}' on the default offering '{Offer}'")]
    internal static partial void TenantStarted(this ILogger<DefaultOffering> logger, TenantId tenant, OfferName offer);

    [LoggerMessage(LogLevel.Debug, "Tenant '{Tenant}' had started on the default offering '{Offer}' already; nothing to tell the operator")]
    internal static partial void AlreadyStarted(this ILogger<DefaultOffering> logger, TenantId tenant, OfferName offer);
}
