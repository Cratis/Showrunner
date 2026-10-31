// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Arc.Queries.ModelBound;
using Cratis.Chronicle.ReadModels;
using Cratis.Showrunner.BoxOffice.Offering.Declaration;
using Cratis.Showrunner.BoxOffice.Portal;

namespace Cratis.Showrunner.BoxOffice.Entitlements;

/// <summary>
/// What the signed-in user's tenant may do today, in one answer for the product's frontend: whether it is read-only and
/// why, whether it is on a trial, how many seats it has and which modules it may use.
/// </summary>
/// <param name="ReadOnlyReason">Why the tenant is read-only; none when it is not.</param>
/// <param name="IsTrial">Whether the tenant's seats come from a trial, closed or not.</param>
/// <param name="TrialEnds">The last day of the trial; nothing when the tenant has no trial.</param>
/// <param name="Seats">How many seats the tenant has today.</param>
/// <param name="SeatsUntil">The last day of the seats; nothing when they do not end, or the tenant has none.</param>
/// <param name="Modules">Every module the product declares, and what the tenant may do with it.</param>
/// <param name="CanOpenPortal">Whether the product has a customer portal to buy, try or change a plan in.</param>
[ReadModel]
public record EntitlementSummary(
    ReadOnlyReason ReadOnlyReason,
    bool IsTrial,
    DateOnly? TrialEnds,
    int Seats,
    DateOnly? SeatsUntil,
    IEnumerable<ModuleStanding> Modules,
    bool CanOpenPortal)
{
    /// <summary>
    /// Gets what the tenant may do today.
    /// </summary>
    /// <param name="entitlements">The tenant's entitlements.</param>
    /// <param name="readModels">The read models.</param>
    /// <param name="offering">What the product declared.</param>
    /// <param name="portal">The customer portal.</param>
    /// <param name="time">The clock.</param>
    /// <returns>The <see cref="EntitlementSummary"/>.</returns>
    [Authorize]
    public static async Task<EntitlementSummary> CurrentEntitlements(
        IEntitlements entitlements,
        IReadModels readModels,
        ProductOffering offering,
        ICustomerPortal portal,
        TimeProvider time)
    {
        var today = DateOnly.FromDateTime(time.GetUtcNow().UtcDateTime);
        var capacity = await readModels.GetInstanceById<SeatCapacity>(TenantStream.Id.Value);
        var modules = await readModels.GetInstanceById<ModuleEntitlements>(TenantStream.Id.Value);

        var isTrial = capacity is not null && capacity.Origin.At > DateTimeOffset.MinValue && capacity.Origin.Value == SeatCapacityOrigin.Trial;
        return new(
            await entitlements.WhyReadOnly(),
            isTrial,
            isTrial ? capacity!.TrialEnds.Value.Value : null,
            await entitlements.SeatCapacity(),
            capacity is not null && !isTrial ? EndOf(capacity.Until.Value) : null,
            [.. offering.Modules
                .OrderBy(_ => _.Name.Value, StringComparer.Ordinal)
                .Select(definition => StandingOf(definition, modules, today))],
            portal.IsAvailable);
    }

    static DateOnly? EndOf(EntitlementEnd end) => end == EntitlementEnd.Never || end.Value == DateOnly.MinValue ? null : end.Value;

    static ModuleStanding StandingOf(ModuleDefinition definition, ModuleEntitlements? modules, DateOnly today)
    {
        var entitlement = modules?.Modules.FirstOrDefault(_ => _.Module == definition.Id);
        var access = entitlement?.On(today) ?? ModuleAccess.None;
        return new(definition.Id, definition.Name, access, access == ModuleAccess.None || entitlement is null ? null : EndOf(entitlement.Until));
    }
}
