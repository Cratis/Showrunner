// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.BoxOffice.Seats.Reporting;

internal static partial class SeatReporterLogging
{
    [LoggerMessage(LogLevel.Warning, "The delivery already reported something other than '{EventType}' for organization '{Organization}' in namespace '{Namespace}'; the seat is reported, so it is not reported again")]
    internal static partial void AlreadyReportedDifferently(this ILogger<SeatReporter> logger, string eventType, OrganizationId organization, EventStoreNamespaceName @namespace);
}
