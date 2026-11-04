// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.BoxOffice.Product;

internal static partial class BoxOfficeProductWorkerLogging
{
    [LoggerMessage(LogLevel.Error, "Box Office failed to listen for the operator's tenants; it tries again at the next round")]
    internal static partial void RoundFailed(this ILogger<BoxOfficeProductWorker> logger, Exception exception);

    [LoggerMessage(LogLevel.Information, "The product does not implement IProvisionTenants, so it creates no tenants when the operator registers them")]
    internal static partial void NotProvisioning(this ILogger<BoxOfficeProductWorker> logger);
}
