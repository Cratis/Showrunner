// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.BoxOffice.Entitlements;
using Cratis.Showrunner.BoxOffice.Seats;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Showrunner.BoxOffice.Suspension;

/// <summary>
/// Refuses the commands a tenant may not execute. A suspended tenant, or one whose trial closed without seats, is
/// read-only: every command is refused with <c>TenantSuspended</c> or <c>TrialClosed</c>, except those marked
/// <see cref="AllowedWhileSuspendedAttribute"/>. A command marked <see cref="RequiresSeatAttribute"/> is refused with
/// <c>SeatRequired</c> when the signed-in user holds no seat the tenant's seats cover.
/// </summary>
public class TenantAccessFilter : ICommandFilter
{
    /// <summary>
    /// The reason a command is refused when the user holds no seat.
    /// </summary>
    public const string SeatRequired = "SeatRequired";

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

        if (RequiresSeat(context.Type))
        {
            var holder = context.ServiceProvider.GetRequiredService<ICurrentSeatHolder>().Current;
            if (holder is null || !await entitlements.HasSeat(holder))
            {
                return CommandResult.Unauthorized(context.CorrelationId, SeatRequired);
            }
        }

        return CommandResult.Success(context.CorrelationId);
    }

    static bool AllowedWhileSuspended(Type command) => Attribute.IsDefined(command, typeof(AllowedWhileSuspendedAttribute), inherit: true);

    static bool RequiresSeat(Type command) => Attribute.IsDefined(command, typeof(RequiresSeatAttribute), inherit: true);
}
