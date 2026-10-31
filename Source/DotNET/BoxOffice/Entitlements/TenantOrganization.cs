// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Entitlements;

/// <summary>
/// The operator's organization behind a tenant, which the product answers under.
/// </summary>
/// <param name="Id">The id of the tenant's stream.</param>
/// <param name="Organization">The operator's identity for the organization.</param>
public record TenantOrganization(string Id, OrganizationId Organization)
{
    /// <summary>
    /// Folds <see cref="TenantOrganizationIdentified"/> into the state.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="current">The current state.</param>
    /// <param name="context">The context of the event.</param>
    /// <returns>The next state.</returns>
    public static TenantOrganization Reduce(TenantOrganizationIdentified @event, TenantOrganization? current, EventContext context) =>
        new(context.EventSourceId.Value, @event.Organization);
}
