// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.GuestList.given;

/// <summary>
/// The product, tenants, users and audiences the specifications use.
/// </summary>
public static class Guests
{
    /// <summary>
    /// The product the specifications' application is.
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
    /// Another user of <see cref="Acme"/>.
    /// </summary>
    public static readonly UserId Bob = "bob";

    /// <summary>
    /// An audience.
    /// </summary>
    public static readonly AudienceName Plus = "Plus";

    /// <summary>
    /// Another audience.
    /// </summary>
    public static readonly AudienceName Preview = "Preview";

    /// <summary>
    /// An audience only the system holds.
    /// </summary>
    public static readonly AudienceName Internal = "Internal";

    /// <summary>
    /// When the first decision is taken.
    /// </summary>
    public static readonly DateTimeOffset Start = new(2026, 11, 2, 9, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// Gets the time of a decision.
    /// </summary>
    /// <param name="minute">The minutes after <see cref="Start"/>.</param>
    /// <returns>The time.</returns>
    public static DateTimeOffset At(int minute) => Start.AddMinutes(minute);

    /// <summary>
    /// Gets a grant at the level of a tenant.
    /// </summary>
    /// <param name="audience">The audience.</param>
    /// <param name="tenant">The tenant; <see cref="Acme"/> when not given.</param>
    /// <returns>The public event.</returns>
    public static AudienceGranted GrantToTenant(AudienceName audience, TenantId? tenant = null) => new(Product, AudienceLevel.Tenant, tenant ?? Acme, UserId.NotSet, audience);

    /// <summary>
    /// Gets a withdrawal at the level of a tenant.
    /// </summary>
    /// <param name="audience">The audience.</param>
    /// <param name="tenant">The tenant; <see cref="Acme"/> when not given.</param>
    /// <returns>The public event.</returns>
    public static AudienceWithdrawn WithdrawFromTenant(AudienceName audience, TenantId? tenant = null) => new(Product, AudienceLevel.Tenant, tenant ?? Acme, UserId.NotSet, audience);

    /// <summary>
    /// Gets a grant at the level of a user of <see cref="Acme"/>.
    /// </summary>
    /// <param name="audience">The audience.</param>
    /// <param name="user">The user; <see cref="Alice"/> when not given.</param>
    /// <returns>The public event.</returns>
    public static AudienceGranted GrantToUser(AudienceName audience, UserId? user = null) => new(Product, AudienceLevel.User, Acme, user ?? Alice, audience);

    /// <summary>
    /// Gets a withdrawal at the level of a user of <see cref="Acme"/>.
    /// </summary>
    /// <param name="audience">The audience.</param>
    /// <param name="user">The user; <see cref="Alice"/> when not given.</param>
    /// <returns>The public event.</returns>
    public static AudienceWithdrawn WithdrawFromUser(AudienceName audience, UserId? user = null) => new(Product, AudienceLevel.User, Acme, user ?? Alice, audience);

    /// <summary>
    /// Gets a grant at the level of the system.
    /// </summary>
    /// <param name="audience">The audience.</param>
    /// <returns>The public event.</returns>
    public static AudienceGranted GrantToSystem(AudienceName audience) => new(Product, AudienceLevel.System, TenantId.NotSet, UserId.NotSet, audience);

    /// <summary>
    /// Gets a withdrawal at the level of the system.
    /// </summary>
    /// <param name="audience">The audience.</param>
    /// <returns>The public event.</returns>
    public static AudienceWithdrawn WithdrawFromSystem(AudienceName audience) => new(Product, AudienceLevel.System, TenantId.NotSet, UserId.NotSet, audience);
}
#endif
