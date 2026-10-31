// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Entitlements;

/// <summary>
/// The modules a tenant has, is trying or may look at. A revoked module stays in the list as the decision that revoked it,
/// so an older grant that arrives late does not bring it back.
/// </summary>
/// <param name="Id">The id of the tenant's stream.</param>
/// <param name="Modules">What the operator last decided about each module.</param>
public record ModuleEntitlements(string Id, IEnumerable<ModuleEntitlement> Modules)
{
    /// <summary>
    /// Gets what the tenant may do with a module on a day.
    /// </summary>
    /// <param name="module">The module.</param>
    /// <param name="today">The day.</param>
    /// <returns>The access.</returns>
    public ModuleAccess AccessTo(ModuleId module, DateOnly today) =>
        Modules.FirstOrDefault(_ => _.Module == module)?.On(today) ?? ModuleAccess.None;

    /// <summary>
    /// Folds <see cref="OperatorGrantedModule"/> into the state.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event.</param>
    /// <returns>The next state.</returns>
    public static ModuleEntitlements Reduce(OperatorGrantedModule @event, ModuleEntitlements? current, EventContext context) =>
        Decide(current, context, new(@event.Module, ModuleAccess.Full, @event.Until, context.Occurred));

    /// <summary>
    /// Folds <see cref="OperatorGrantedModuleTrial"/> into the state.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event.</param>
    /// <returns>The next state.</returns>
    public static ModuleEntitlements Reduce(OperatorGrantedModuleTrial @event, ModuleEntitlements? current, EventContext context) =>
        Decide(current, context, new(@event.Module, ModuleAccess.Trial, new(@event.Ends.Value), context.Occurred));

    /// <summary>
    /// Folds <see cref="OperatorOpenedModulePreview"/> into the state.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event.</param>
    /// <returns>The next state.</returns>
    public static ModuleEntitlements Reduce(OperatorOpenedModulePreview @event, ModuleEntitlements? current, EventContext context) =>
        Decide(current, context, new(@event.Module, ModuleAccess.Preview, @event.Until, context.Occurred));

    /// <summary>
    /// Folds <see cref="OperatorRevokedModule"/> into the state.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event.</param>
    /// <returns>The next state.</returns>
    public static ModuleEntitlements Reduce(OperatorRevokedModule @event, ModuleEntitlements? current, EventContext context) =>
        Decide(current, context, new(@event.Module, ModuleAccess.None, EntitlementEnd.Never, context.Occurred));

    static ModuleEntitlements Decide(ModuleEntitlements? current, EventContext context, ModuleEntitlement decision)
    {
        var standing = current?.Modules ?? [];
        if (standing.FirstOrDefault(_ => _.Module == decision.Module) is { } earlier && earlier.DecidedAt > decision.DecidedAt)
        {
            return current!;
        }

        return new(context.EventSourceId.Value, [.. standing.Where(_ => _.Module != decision.Module), decision]);
    }
}
