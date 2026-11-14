// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.GuestList.Authoring.Publishing;

/// <summary>
/// Turns what an operator decided into the public events a product receives. Each method is one row of the mapping table
/// in the authoring documentation.
/// </summary>
public static class PublicEvents
{
    /// <summary>
    /// Maps an audience being granted.
    /// </summary>
    /// <param name="product">The product the decision is for.</param>
    /// <param name="event">The private event.</param>
    /// <returns>The public event.</returns>
    public static AudienceGranted For(Product product, AudienceGrantDecided @event) => new(product, @event.Level, @event.TenantId, @event.UserId, @event.Audience, @event.ClientId, @event.Kind);

    /// <summary>
    /// Maps an audience being withdrawn.
    /// </summary>
    /// <param name="product">The product the decision is for.</param>
    /// <param name="event">The private event.</param>
    /// <returns>The public event.</returns>
    public static AudienceWithdrawn For(Product product, AudienceWithdrawalDecided @event) => new(product, @event.Level, @event.TenantId, @event.UserId, @event.Audience, @event.ClientId, @event.Kind);
}
