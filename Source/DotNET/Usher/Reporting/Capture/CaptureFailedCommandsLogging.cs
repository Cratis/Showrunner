// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.Usher.Reporting.Capture;

/// <summary>
/// Log messages for <see cref="CaptureFailedCommands"/>.
/// </summary>
internal static partial class CaptureFailedCommandsLogging
{
    [LoggerMessage(LogLevel.Warning, "Usher could not capture the error of the failing command with correlation id {CorrelationId}; a report of it is recorded from what the client sends")]
    internal static partial void CaptureFailed(this ILogger<CaptureFailedCommands> logger, Guid correlationId, Exception exception);
}
