// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Watchdog;

internal static partial class WatchdogLogMessages
{
    [LoggerMessage(LogLevel.Error, "configuration: {Problem}")]
    internal static partial void ConfigurationProblem(this ILogger logger, string problem);
}
