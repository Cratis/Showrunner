// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.ReadModels;
using Cratis.Chronicle.Transactions;
using Cratis.Showrunner.BoxOffice.Seats;

namespace Cratis.Showrunner.BoxOffice.Entitlements;

/// <summary>
/// Answers what the current tenant may use from what the operator decided. The read models are those of the current
/// request's namespace, which is the tenant.
/// </summary>
/// <param name="readModels">The <see cref="IReadModels"/> of the current tenant.</param>
/// <param name="unitOfWorkManager">The <see cref="IUnitOfWorkManager"/> of the current tenant, which says whether the command is protected.</param>
/// <param name="time">The <see cref="TimeProvider"/> the days are counted by.</param>
public class TenantEntitlements(IReadModels readModels, IUnitOfWorkManager unitOfWorkManager, TimeProvider time) : IEntitlements
{
    DateOnly Today => DateOnly.FromDateTime(time.GetUtcNow().UtcDateTime);

    /// <inheritdoc/>
    public async Task<bool> IsActive() => await WhyReadOnly() == ReadOnlyReason.None;

    /// <inheritdoc/>
    public async Task<ReadOnlyReason> WhyReadOnly()
    {
        var standing = await Read<TenantStanding>();
        if (standing?.Suspended == true)
        {
            return ReadOnlyReason.TenantSuspended;
        }

        var capacity = await Read<SeatCapacity>();
        return capacity?.IsTrialOver(Today) == true ? ReadOnlyReason.TrialClosed : ReadOnlyReason.None;
    }

    /// <inheritdoc/>
    public async Task<bool> HasSeat(SeatHolder subject)
    {
        var roster = await Read<SeatRoster>();
        var capacity = await SeatCapacity();
        return roster?.IsLicensed(subject, capacity) == true;
    }

    /// <inheritdoc/>
    public async Task<bool> Has(ModuleId module) => await AccessTo(module) is ModuleAccess.Trial or ModuleAccess.Full;

    /// <inheritdoc/>
    public async Task<ModuleAccess> AccessTo(ModuleId module) =>
        (await Read<ModuleEntitlements>())?.AccessTo(module, Today) ?? ModuleAccess.None;

    /// <inheritdoc/>
    public async Task<MeterAllowance> AllowanceFor(MeterId meter) =>
        (await Read<Allowances>())?.For(meter) ?? MeterAllowance.None(meter);

    /// <inheritdoc/>
    public async Task<int> SeatCapacity() => (await Read<SeatCapacity>())?.On(Today) ?? 0;

    /// <summary>
    /// Reads a read model of the tenant. Admission is decided before the command decides anything, so the read is advisory
    /// by nature and never a guard: under read protection <c>On</c> it is declared as such, because a read the command did
    /// not enroll is otherwise refused.
    /// </summary>
    /// <typeparam name="T">The read model.</typeparam>
    /// <returns>The read model, or <see langword="null"/> when nothing was recorded.</returns>
    async Task<T?> Read<T>()
        where T : class
    {
        using var advisory = unitOfWorkManager.HasCurrent
            ? unitOfWorkManager.Current.AllowReads(ReadAllowance.Advisory, "Admission to a command is advisory; a command guards what it decides with the facts it reads itself.")
            : null;
        return await readModels.GetInstanceById<T>(TenantStream.Id.Value);
    }
}
