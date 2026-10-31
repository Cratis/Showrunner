// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.BoxOffice.Contracts.Offers;

namespace Cratis.Showrunner.BoxOffice.Receiving;

/// <summary>
/// The operator's public decisions Box Office receives, and the private fact each one becomes.
/// </summary>
public static class OperatorDecisions
{
    /// <summary>
    /// The public events of the operator that concern one tenant.
    /// </summary>
    public static readonly Type[] ForTenants =
    [
        typeof(TrialOpened),
        typeof(TrialEndMoved),
        typeof(TrialClosed),
        typeof(SeatCapacitySet),
        typeof(ModuleGranted),
        typeof(ModuleTrialGranted),
        typeof(ModulePreviewOpened),
        typeof(ModuleRevoked),
        typeof(AllowanceSet),
        typeof(TenantSuspended),
        typeof(TenantReinstated),
    ];

    /// <summary>
    /// The public events of the operator that concern the product as a whole.
    /// </summary>
    public static readonly Type[] ForTheProduct =
    [
        typeof(OfferPublished),
        typeof(OfferWithdrawn),
    ];

    /// <summary>
    /// Gets every public event of the operator Box Office subscribes to.
    /// </summary>
    public static IEnumerable<Type> All => [.. ForTenants, .. ForTheProduct];

    /// <summary>
    /// Maps a decision about a tenant to the tenant and the private fact it becomes.
    /// </summary>
    /// <param name="decision">The public event the operator sent.</param>
    /// <returns>The tenant and the fact, or <see langword="null"/> when the event does not concern a tenant.</returns>
    public static (TenantId TenantId, object Fact)? ForTenant(object decision) => decision switch
    {
        TrialOpened e => (e.TenantId, new OperatorOpenedTrial(e.Ends, e.Seats)),
        TrialEndMoved e => (e.TenantId, new OperatorMovedTrialEnd(e.Ends)),
        TrialClosed e => (e.TenantId, new OperatorClosedTrial(e.Closure)),
        SeatCapacitySet e => (e.TenantId, new OperatorSetSeatCapacity(e.Capacity, e.Until, e.Source)),
        ModuleGranted e => (e.TenantId, new OperatorGrantedModule(e.Module, e.Until)),
        ModuleTrialGranted e => (e.TenantId, new OperatorGrantedModuleTrial(e.Module, e.Ends)),
        ModulePreviewOpened e => (e.TenantId, new OperatorOpenedModulePreview(e.Module, e.Until)),
        ModuleRevoked e => (e.TenantId, new OperatorRevokedModule(e.Module)),
        AllowanceSet e => (e.TenantId, new OperatorSetAllowance(e.Meter, e.Quantity, e.Period, e.Overage)),
        TenantSuspended e => (e.TenantId, new OperatorSuspendedTenant(e.Cause)),
        TenantReinstated e => (e.TenantId, new OperatorReinstatedTenant()),
        _ => null
    };

    /// <summary>
    /// Maps a decision about the product to the private fact it becomes.
    /// </summary>
    /// <param name="decision">The public event the operator sent.</param>
    /// <returns>The fact, or <see langword="null"/> when the event does not concern the product as a whole.</returns>
    public static object? ForProduct(object decision) => decision switch
    {
        OfferPublished e => new OperatorPublishedOffer(e.Name, e.Modules, e.IncludedSeats, e.Prices),
        OfferWithdrawn => new OperatorWithdrewOffer(),
        _ => null
    };
}
