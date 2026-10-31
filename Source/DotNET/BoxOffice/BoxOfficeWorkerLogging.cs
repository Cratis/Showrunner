// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.BoxOffice;

internal static partial class BoxOfficeWorkerLogging
{
    [LoggerMessage(LogLevel.Error, "Box Office failed to serve the tenant namespaces; it tries again at the next round")]
    internal static partial void RoundFailed(this ILogger<BoxOfficeWorker> logger, Exception exception);
}
