// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Product.Provisioning;

/// <summary>
/// The person who registered a tenant, in the tenant's own namespace.
/// </summary>
/// <param name="Id">The key of the read model: the tenant's stream.</param>
/// <param name="Person">The person who registered the tenant.</param>
/// <param name="Role">The role the creator holds in the tenant.</param>
public record TenantCreator(string Id, Person Person, TenantRole Role)
{
    /// <summary>
    /// Reduces the creator being identified.
    /// </summary>
    /// <param name="event">The <see cref="TenantCreatorIdentified"/> event.</param>
    /// <param name="current">The creator so far, if any.</param>
    /// <param name="context">The <see cref="EventContext"/> of the event.</param>
    /// <returns>The new <see cref="TenantCreator"/>.</returns>
    public static TenantCreator Reduce(TenantCreatorIdentified @event, TenantCreator? current, EventContext context) =>
        (current ?? new(context.EventSourceId.Value, @event.Creator, @event.Role)) with { Person = @event.Creator, Role = @event.Role };
}
