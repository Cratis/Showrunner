// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.Herald;

/// <summary>
/// Log messages for <see cref="HeraldWorker"/>.
/// </summary>
internal static partial class HeraldWorkerLogging
{
    [LoggerMessage(LogLevel.Error, "A round of Herald's email delivery failed with {ExceptionType}; the next round tries again")]
    internal static partial void RoundFailed(this ILogger<HeraldWorker> logger, string exceptionType);
}
