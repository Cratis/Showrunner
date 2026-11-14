// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.GuestList.Authoring.Tenancy;

namespace Cratis.Showrunner.GuestList.Authoring.WithdrawAudience;

/// <summary>
/// Withdraws an audience from a tenant, a user of a tenant, or the system.
/// </summary>
/// <remarks>
/// A withdrawal removes the grant of its own level only: withdrawing an audience from a user leaves the tenant's and the
/// system's grants of it in place. Withdrawing an audience that is not held at that level records nothing, so a repeated
/// command is harmless and the decision is published once. Which fields name whom depends on the level, as for granting.
/// </remarks>
/// <param name="Product">The product the audience is for.</param>
/// <param name="Level">Whom the audience is withdrawn from.</param>
/// <param name="TenantId">The tenant, for a tenant or a user grant; empty for the system.</param>
/// <param name="UserId">The user, for a user grant; empty otherwise.</param>
/// <param name="Audience">The audience to withdraw.</param>
/// <param name="ClientId">The client id of the system, for a named system; empty otherwise.</param>
/// <param name="Kind">The kind of the system, for a named system; none otherwise.</param>
[Command]
[Authorize(Policy = GuestListAuthoring.Policy)]
public record WithdrawAudience(Product Product, AudienceLevel Level, TenantId TenantId, UserId UserId, AudienceName Audience, SystemClientId? ClientId = null, SystemKind Kind = SystemKind.None) : IAudienceDecision, ICanProvideEventSourceId
{
    /// <inheritdoc/>
    public EventSourceId GetEventSourceId() => new(Product.Value);

    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <param name="guestList">What is held for the product, read fresh from the event log.</param>
    /// <returns><see cref="AudienceWithdrawalDecided"/>, or nothing when the audience is not held.</returns>
    public IEnumerable<object> Handle(ProductGuestList? guestList) =>
        guestList?.Holds(this.Held()) == true ? [new AudienceWithdrawalDecided(Level, TenantId, UserId, Audience, ClientId ?? SystemClientId.NotSet, Kind)] : [];
}

/// <summary>
/// Validates <see cref="WithdrawAudience"/>.
/// </summary>
public class WithdrawAudienceValidator : CommandValidator<WithdrawAudience>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WithdrawAudienceValidator"/> class.
    /// </summary>
    /// <param name="tenants">What the operator side knows of the tenants of a product.</param>
    public WithdrawAudienceValidator(IKnownTenants tenants) => this.AddAudienceDecisionRules(tenants);
}
