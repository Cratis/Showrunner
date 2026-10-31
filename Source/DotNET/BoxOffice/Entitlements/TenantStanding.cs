// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Arc.Queries.ModelBound;
using Cratis.Chronicle.ReadModels;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Entitlements;

/// <summary>
/// Whether the operator suspended a tenant, and the reason the tenant is read-only when it is, for the frontend to show.
/// </summary>
/// <param name="Id">The id of the tenant's stream.</param>
/// <param name="Suspended">Whether the operator has the tenant suspended.</param>
/// <param name="Cause">Why the operator suspended the tenant, while it does.</param>
/// <param name="DecidedAt">When the operator made the decision that stands. A decision that arrives after a newer one is ignored.</param>
/// <param name="ReadOnlyReason">Why the tenant can read but not change anything, evaluated when it is read; none when it can.</param>
[ReadModel]
public record TenantStanding(string Id, bool Suspended, SuspensionCause Cause, DateTimeOffset DecidedAt, ReadOnlyReason ReadOnlyReason)
{
    /// <summary>
    /// Gets why the tenant can not change anything, so the frontend can show a banner.
    /// </summary>
    /// <param name="entitlements">The <see cref="IEntitlements"/> of the current tenant.</param>
    /// <param name="readModels">The <see cref="IReadModels"/> of the current tenant.</param>
    /// <returns>The standing, with the reason evaluated now.</returns>
    [Authorize]
    public static async Task<TenantStanding> CurrentStanding(IEntitlements entitlements, IReadModels readModels)
    {
        var standing = await readModels.GetInstanceById<TenantStanding>(TenantStream.Id.Value) ?? TenantStandingState.Unsuspended(TenantStream.Id.Value);
        return standing with { ReadOnlyReason = await entitlements.WhyReadOnly() };
    }

    /// <summary>
    /// Folds <see cref="OperatorSuspendedTenant"/> into the state.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event.</param>
    /// <returns>The next state.</returns>
    public static TenantStanding Reduce(OperatorSuspendedTenant @event, TenantStanding? current, EventContext context)
    {
        var standing = current ?? TenantStandingState.Unsuspended(context.EventSourceId.Value);
        return context.Occurred < standing.DecidedAt ? standing : standing with { Suspended = true, Cause = @event.Cause, DecidedAt = context.Occurred };
    }

    /// <summary>
    /// Folds <see cref="OperatorReinstatedTenant"/> into the state.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event.</param>
    /// <returns>The next state.</returns>
    public static TenantStanding Reduce(OperatorReinstatedTenant @event, TenantStanding? current, EventContext context)
    {
        var standing = current ?? TenantStandingState.Unsuspended(context.EventSourceId.Value);
        return context.Occurred < standing.DecidedAt ? standing : standing with { Suspended = false, DecidedAt = context.Occurred };
    }
}

/// <summary>
/// The state of a tenant the operator never suspended.
/// </summary>
internal static class TenantStandingState
{
    /// <summary>
    /// Gets the state of a tenant the operator never suspended.
    /// </summary>
    /// <param name="id">The id of the tenant's stream.</param>
    /// <returns>The state.</returns>
    internal static TenantStanding Unsuspended(string id) =>
        new(id, false, SuspensionCause.AdministratorDecision, DateTimeOffset.MinValue, ReadOnlyReason.None);
}
