// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.GuestList.Authoring.Tenancy;

namespace Cratis.Showrunner.GuestList.Authoring.GrantAudience;

/// <summary>
/// Grants an audience to a tenant, a user of a tenant, the system, or one named system.
/// </summary>
/// <remarks>
/// Granting an audience that is already held at that level records nothing, so a repeated command is harmless and the
/// decision is published once. Which fields name whom depends on the level: a tenant grant names the tenant, a user grant
/// names the tenant and the user, a system grant names neither, and a named-system grant names the client id and the kind of the
/// system and no tenant or user. A system grant applies to every system; a named-system grant adds to it for one system.
/// Anything else is refused. The audience need not be one
/// the product declares: a grant is kept, and the undeclared grants query reports it.
/// </remarks>
/// <param name="Product">The product the audience is for.</param>
/// <param name="Level">Whom the audience is granted to.</param>
/// <param name="TenantId">The tenant, for a tenant or a user grant; empty for the system.</param>
/// <param name="UserId">The user, for a user grant; empty otherwise.</param>
/// <param name="Audience">The audience to grant, as the model names it.</param>
/// <param name="ClientId">The client id of the system, for a named system; empty otherwise.</param>
/// <param name="Kind">The kind of the system, for a named system; none otherwise.</param>
[Command]
[Authorize(Policy = GuestListAuthoring.Policy)]
public record GrantAudience(Product Product, AudienceLevel Level, TenantId TenantId, UserId UserId, AudienceName Audience, SystemClientId? ClientId = null, SystemKind Kind = SystemKind.None) : IAudienceDecision, ICanProvideEventSourceId
{
    /// <inheritdoc/>
    public EventSourceId GetEventSourceId() => new(Product.Value);

    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <param name="guestList">What is held for the product, read fresh from the event log.</param>
    /// <returns><see cref="AudienceGrantDecided"/>, or nothing when the audience is already held.</returns>
    public IEnumerable<object> Handle(ProductGuestList? guestList) =>
        guestList?.Holds(this.Held()) == true ? [] : [new AudienceGrantDecided(Level, TenantId, UserId, Audience, ClientId ?? SystemClientId.NotSet, Kind)];
}

/// <summary>
/// Validates <see cref="GrantAudience"/>.
/// </summary>
public class GrantAudienceValidator : CommandValidator<GrantAudience>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GrantAudienceValidator"/> class.
    /// </summary>
    /// <param name="tenants">What the operator side knows of the tenants of a product.</param>
    public GrantAudienceValidator(IKnownTenants tenants) => this.AddAudienceDecisionRules(tenants);
}
