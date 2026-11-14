// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Choreography.Availability.Audiences;
using Cratis.Showrunner.GuestList.Querying;
using ChoreographyAudience = Cratis.Choreography.Availability.Audiences.AudienceName;
using ChoreographyLevel = Cratis.Choreography.Availability.Audiences.AudienceLevel;
using ShowrunnerAudience = Cratis.Showrunner.GuestList.Contracts.Audiences.AudienceName;
using SystemClientId = Cratis.Showrunner.GuestList.Contracts.Audiences.SystemClientId;
using TenantId = Cratis.Showrunner.GuestList.Contracts.Audiences.TenantId;
using UserId = Cratis.Showrunner.GuestList.Contracts.Audiences.UserId;

namespace Cratis.Showrunner.GuestList.Availability;

/// <summary>
/// Represents an <see cref="IAudienceSource"/> over what the product recorded in its own event store, which is what the
/// availability engine reads to know the audiences of a request.
/// </summary>
/// <remarks>
/// <para>
/// The effective audiences are the union of the tenant's, the user's and the system's. A system grant applies to every
/// tenant of the product, so it is part of every answer; a grant to one named system adds to it for the caller that names
/// that client id, through <see cref="GetGrants(AudienceSubject, SystemClientId, CancellationToken)"/>. Each grant is tagged with the level it is held at; a withdrawal at
/// one level never touches another.
/// </para>
/// <para>
/// Nothing is asked of the operator. The answer is read from the local read models, so it is the last state the product
/// received and stays so while the link to the operator is down. Grants for audiences the deployed model does not declare
/// are returned like any other: the engine carries them, never matches a gate with them and reports them.
/// </para>
/// </remarks>
/// <param name="guestList">What the product knows about the audiences of its tenants, users and the system.</param>
/// <param name="changes">The feed of changes to what the product holds.</param>
public class GuestListAudienceSource(IGuestList guestList, AudienceChangeFeed changes) : IAudienceSource
{
    /// <inheritdoc/>
    public ValueTask<IReadOnlyCollection<AudienceGrant>> GetGrants(AudienceSubject subject, CancellationToken cancellationToken = default) =>
        GetGrants(subject, ClientOf(subject), cancellationToken);

    /// <summary>
    /// Gets the grants that apply to a request made by a system that names its client id: the audiences granted to every
    /// system and the ones granted to this system, and the tenant's and the user's the subject names. A grant to another named
    /// system is never part of the answer, and a caller with no client id gets the audiences granted to every system only.
    /// </summary>
    /// <param name="subject">Who the request is for.</param>
    /// <param name="client">The client id of the calling system; <see cref="SystemClientId.NotSet"/> when it names none.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The grants, each tagged with the level it is held at.</returns>
    public async ValueTask<IReadOnlyCollection<AudienceGrant>> GetGrants(AudienceSubject subject, SystemClientId client, CancellationToken cancellationToken = default)
    {
        List<AudienceGrant> grants = [];
        grants.AddRange(Tag(await guestList.ForSystemCaller(client, cancellationToken), ChoreographyLevel.System));

        if (string.IsNullOrWhiteSpace(subject.Tenant))
        {
            return grants;
        }

        var tenant = new TenantId(subject.Tenant);
        grants.AddRange(Tag(await guestList.ForTenant(tenant, cancellationToken), ChoreographyLevel.Tenant));

        if (!string.IsNullOrWhiteSpace(subject.User))
        {
            grants.AddRange(Tag(await guestList.ForUser(tenant, new UserId(subject.User), cancellationToken), ChoreographyLevel.User));
        }

        return grants;
    }

    /// <inheritdoc/>
    public IDisposable OnChanged(Func<AudienceChange, ValueTask> handler) => changes.Subscribe(handler);

    static SystemClientId ClientOf(AudienceSubject subject) =>
        subject.IsSystem && !string.IsNullOrWhiteSpace(subject.SystemClientId) ? new SystemClientId(subject.SystemClientId) : SystemClientId.NotSet;

    static IEnumerable<AudienceGrant> Tag(IEnumerable<ShowrunnerAudience> audiences, ChoreographyLevel level) =>
        audiences.Select(audience => new AudienceGrant(new ChoreographyAudience(audience.Value), level));
}
