// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.Notes.Collecting;

/// <summary>
/// Log messages for <see cref="CollectingWorker"/>.
/// </summary>
internal static partial class CollectingWorkerLogging
{
    [LoggerMessage(LogLevel.Warning, "The collecting background round failed; it runs again after the polling interval")]
    internal static partial void RoundFailed(this ILogger<CollectingWorker> logger, Exception exception);
}
