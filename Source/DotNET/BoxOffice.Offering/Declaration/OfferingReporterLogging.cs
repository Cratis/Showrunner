// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.BoxOffice.Offering.Declaration;

/// <summary>
/// Log messages for <see cref="OfferingReporter"/> and <see cref="OfferingReportingService"/>.
/// </summary>
internal static partial class OfferingReporterLogging
{
    [LoggerMessage(LogLevel.Information, "Reported the offering of product '{Product}': {Modules} module(s) and {Meters} meter(s)")]
    internal static partial void OfferingReported(this ILogger<OfferingReporter> logger, ProductId product, int modules, int meters);

    [LoggerMessage(LogLevel.Debug, "The offering of product '{Product}' is what was last reported; nothing to report")]
    internal static partial void OfferingUnchanged(this ILogger<OfferingReporter> logger, ProductId product);

    [LoggerMessage(LogLevel.Warning, "Could not report the offering; trying again soon")]
    internal static partial void ReportingFailed(this ILogger<OfferingReportingService> logger, Exception exception);
}
