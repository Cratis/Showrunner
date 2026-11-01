// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.Notes.Tenancy;

/// <summary>
/// Log messages for <see cref="NotesWorker"/>.
/// </summary>
internal static partial class NotesWorkerLogging
{
    [LoggerMessage(LogLevel.Warning, "Notes' background round failed; it runs again after the polling interval")]
    internal static partial void RoundFailed(this ILogger<NotesWorker> logger, Exception exception);
}
