// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.GuestList;

/// <summary>
/// Log messages for <see cref="GuestListWorker"/>.
/// </summary>
internal static partial class GuestListWorkerLogging
{
    [LoggerMessage(LogLevel.Error, "A round of GuestList's background work failed; the next round tries again")]
    internal static partial void RoundFailed(this ILogger<GuestListWorker> logger, Exception exception);
}
