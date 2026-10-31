// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.BoxOffice.Compute.Reporting;

internal static partial class ComputeReporterLogging
{
    [LoggerMessage(LogLevel.Warning, "The delivery already reported something else for organization '{Organization}' in namespace '{Namespace}'; the allowance running out is reported, so it is not reported again")]
    internal static partial void AlreadyReportedDifferently(this ILogger<ComputeReporter> logger, OrganizationId organization, EventStoreNamespaceName @namespace);
}
