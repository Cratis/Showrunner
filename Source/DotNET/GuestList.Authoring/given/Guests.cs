// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.GuestList.Authoring.given;

/// <summary>
/// The product, tenants, users and audiences the specifications use, and the decisions they seed.
/// </summary>
public static class Guests
{
    /// <summary>
    /// The product the operator decides for.
    /// </summary>
    public static readonly Product Product = "ledger";

    /// <summary>
    /// A tenant of the product.
    /// </summary>
    public static readonly TenantId Acme = "acme";

    /// <summary>
    /// Another tenant of the product.
    /// </summary>
    public static readonly TenantId Globex = "globex";

    /// <summary>
    /// A user of <see cref="Acme"/>.
    /// </summary>
    public static readonly UserId Alice = "alice";

    /// <summary>
    /// An audience.
    /// </summary>
    public static readonly AudienceName Plus = "Plus";

    /// <summary>
    /// Another audience.
    /// </summary>
    public static readonly AudienceName Preview = "Preview";

    /// <summary>
    /// The decision that grants <see cref="Plus"/> to <see cref="Acme"/>.
    /// </summary>
    public static readonly AudienceGrantDecided TenantGrant = new(AudienceLevel.Tenant, Acme, UserId.NotSet, Plus);

    /// <summary>
    /// The decision that withdraws <see cref="Plus"/> from <see cref="Acme"/>.
    /// </summary>
    public static readonly AudienceWithdrawalDecided TenantWithdrawal = new(AudienceLevel.Tenant, Acme, UserId.NotSet, Plus);

    /// <summary>
    /// The decision that grants <see cref="Plus"/> to <see cref="Alice"/> of <see cref="Acme"/>.
    /// </summary>
    public static readonly AudienceGrantDecided UserGrant = new(AudienceLevel.User, Acme, Alice, Plus);

    /// <summary>
    /// The decision that grants <see cref="Plus"/> to the system.
    /// </summary>
    public static readonly AudienceGrantDecided SystemGrant = new(AudienceLevel.System, TenantId.NotSet, UserId.NotSet, Plus);
}
#endif
