// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.BoxOffice.Gating;

internal static partial class ModuleGateLogging
{
    [LoggerMessage(LogLevel.Information, "Refused '{Name}': the tenant does not have module '{Module}'")]
    internal static partial void ModuleNotEntitled(this ILogger<ModuleGate> logger, ModuleId module, string name);
}
