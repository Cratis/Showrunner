// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.BoxOffice.Seats;

namespace Cratis.Showrunner.BoxOffice.Entitlements;

/// <summary>
/// Answers what the current tenant may use. What the operator decided is read when asked, and a trial, a module or a
/// seat that has ended stops counting the moment its last day passes, without an event.
/// </summary>
public interface IEntitlements
{
    /// <summary>
    /// Gets whether the tenant can change things: it is not suspended and has not been left read-only by a trial that
    /// closed without seats.
    /// </summary>
    /// <returns><see langword="true"/> when the tenant can change things.</returns>
    Task<bool> IsActive();

    /// <summary>
    /// Gets why the tenant can not change things.
    /// </summary>
    /// <returns>The reason, <see cref="ReadOnlyReason.None"/> when the tenant can change things.</returns>
    Task<ReadOnlyReason> WhyReadOnly();

    /// <summary>
    /// Gets whether a person holds a seat the tenant's capacity covers.
    /// </summary>
    /// <param name="subject">Who.</param>
    /// <returns><see langword="true"/> when they hold a seat that is licensed.</returns>
    Task<bool> HasSeat(SeatHolder subject);

    /// <summary>
    /// Gets whether the tenant has a module, in full or on trial. A preview does not count.
    /// </summary>
    /// <param name="module">The module.</param>
    /// <returns><see langword="true"/> when the tenant may use the module.</returns>
    Task<bool> Has(ModuleId module);

    /// <summary>
    /// Gets what the tenant may do with a module.
    /// </summary>
    /// <param name="module">The module.</param>
    /// <returns>The access.</returns>
    Task<ModuleAccess> AccessTo(ModuleId module);

    /// <summary>
    /// Gets what the tenant may use of a meter.
    /// </summary>
    /// <param name="meter">The meter.</param>
    /// <returns>The allowance, nothing when the operator set none.</returns>
    Task<MeterAllowance> AllowanceFor(MeterId meter);

    /// <summary>
    /// Gets how many seats the tenant may use.
    /// </summary>
    /// <returns>The number of seats.</returns>
    Task<int> SeatCapacity();
}
