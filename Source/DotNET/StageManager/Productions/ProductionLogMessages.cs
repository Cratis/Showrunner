// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// Log messages of the Production reconciliation. None of them carries a secret value.
/// </summary>
public static partial class ProductionLogMessages
{
    [LoggerMessage(LogLevel.Information, "Production {Namespace}/{Production} is {Phase}")]
    internal static partial void ProductionPhase(this ILogger logger, string @namespace, string production, ProductionPhase phase);

    [LoggerMessage(LogLevel.Error, "Reconciling Production {Namespace}/{Production} failed")]
    internal static partial void ReconciliationFailed(this ILogger logger, string @namespace, string production, Exception exception);
}
