// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.GuestList.Authoring.Tenancy;

namespace Cratis.Showrunner.GuestList.Authoring.SuspendPart;

/// <summary>
/// Suspends a part of the product for a tenant, a user of a tenant, or the system.
/// </summary>
/// <remarks>
/// Suspending a part that is already suspended at that level records nothing, so a repeated command is harmless and the
/// decision is published once. Which fields name whom depends on the level: a tenant suspension names the tenant, a user
/// suspension names the tenant and the user, a system suspension names neither. Anything else is refused. The part is a
/// path from the declaration report, for example <c>Fleet/DataQuality</c>, and covers everything below it. A suspension
/// wins over every audience a caller holds.
/// </remarks>
/// <param name="Product">The product the part is in.</param>
/// <param name="Level">Whom the part is suspended for.</param>
/// <param name="TenantId">The tenant, for a tenant or a user suspension; empty for the system.</param>
/// <param name="UserId">The user, for a user suspension; empty otherwise.</param>
/// <param name="Part">The path of the part to suspend.</param>
[Command]
[Authorize(Policy = GuestListAuthoring.Policy)]
public record SuspendPart(Product Product, SuspensionLevel Level, TenantId TenantId, UserId UserId, PartPath Part) : ISuspensionDecision, ICanProvideEventSourceId
{
    /// <inheritdoc/>
    public EventSourceId GetEventSourceId() => new(Product.Value);

    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <param name="suspensions">What is suspended for the product, read fresh from the event log.</param>
    /// <returns><see cref="PartSuspensionDecided"/>, or nothing when the part is already suspended at that level.</returns>
    public IEnumerable<object> Handle(ProductSuspensions? suspensions) =>
        suspensions?.IsSuspended(Level, TenantId, UserId, Part) == true ? [] : [new PartSuspensionDecided(Level, TenantId, UserId, Part)];
}

/// <summary>
/// Validates <see cref="SuspendPart"/>.
/// </summary>
public class SuspendPartValidator : CommandValidator<SuspendPart>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SuspendPartValidator"/> class.
    /// </summary>
    /// <param name="tenants">What the operator side knows of the tenants of a product.</param>
    public SuspendPartValidator(IKnownTenants tenants) => this.AddSuspensionDecisionRules(tenants);
}
