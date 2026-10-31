// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.BoxOffice.Entitlements;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Showrunner.BoxOffice.Suspension;

/// <summary>
/// Refuses the commands a tenant may not execute. A suspended tenant, or one whose trial closed without seats, is
/// read-only: every command is refused with <c>TenantSuspended</c> or <c>TrialClosed</c>, except those marked
/// <see cref="AllowedWhileSuspendedAttribute"/>.
/// </summary>
public class TenantAccessFilter : ICommandFilter
{
    /// <inheritdoc/>
    public async Task<CommandResult> OnExecution(CommandContext context)
    {
        if (context.ServiceProvider?.GetService<IEntitlements>() is not { } entitlements)
        {
            return CommandResult.Success(context.CorrelationId);
        }

        if (!AllowedWhileSuspended(context.Type))
        {
            var reason = await entitlements.WhyReadOnly();
            if (reason != ReadOnlyReason.None)
            {
                return CommandResult.Unauthorized(context.CorrelationId, reason.ToString());
            }
        }

        return CommandResult.Success(context.CorrelationId);
    }

    static bool AllowedWhileSuspended(Type command) => Attribute.IsDefined(command, typeof(AllowedWhileSuspendedAttribute), inherit: true);
}
