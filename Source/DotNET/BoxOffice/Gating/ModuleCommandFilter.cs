// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Showrunner.BoxOffice.Gating;

/// <summary>
/// Refuses a command that belongs to a module the tenant does not have, with <see cref="GatingReasons.ModuleNotEntitled"/>.
/// It refuses the product's own background flows alike, because a module the tenant does not have acts for nobody.
/// </summary>
public class ModuleCommandFilter : ICommandFilter
{
    /// <inheritdoc/>
    public async Task<CommandResult> OnExecution(CommandContext context)
    {
        if (context.ServiceProvider?.GetService<IModuleGate>() is not { } gate)
        {
            return CommandResult.Success(context.CorrelationId);
        }

        return await gate.RefusedModuleOf(context.Type.Namespace) is null
            ? CommandResult.Success(context.CorrelationId)
            : CommandResult.Unauthorized(context.CorrelationId, GatingReasons.ModuleNotEntitled);
    }
}
