// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// Log messages of the Performance reconciliation. None of them carries a secret value or a resolved environment value.
/// </summary>
public static partial class PerformanceLogMessages
{
    [LoggerMessage(LogLevel.Warning, "The environment {Environment} could not be resolved ({ExceptionType})")]
    internal static partial void EnvironmentNotResolved(this ILogger logger, string environment, string exceptionType);

    [LoggerMessage(LogLevel.Information, "Performance {Namespace}/{Performance} is {Phase}")]
    internal static partial void PerformancePhase(this ILogger logger, string @namespace, string performance, PerformancePhase phase);

    [LoggerMessage(LogLevel.Error, "Reconciling Performance {Namespace}/{Performance} failed")]
    internal static partial void ReconciliationFailed(this ILogger logger, string @namespace, string performance, Exception exception);
}
