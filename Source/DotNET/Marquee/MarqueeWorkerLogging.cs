// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.Marquee;

/// <summary>
/// Log messages for <see cref="MarqueeWorker"/>.
/// </summary>
internal static partial class MarqueeWorkerLogging
{
    [LoggerMessage(LogLevel.Error, "A round of Marquee's background work failed; the next round tries again")]
    internal static partial void RoundFailed(this ILogger<MarqueeWorker> logger, Exception exception);
}
