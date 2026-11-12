// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Arc.Queries.ModelBound;
using Microsoft.Extensions.Options;
using ITenantIdAccessor = Cratis.Arc.Tenancy.ITenantIdAccessor;

namespace Cratis.Showrunner.GuestList.Querying;

/// <summary>
/// A grant of an audience the application does not declare. The product keeps it, because a decision is a fact, but it
/// never matches a gate. Declaring the audience, or withdrawing the grant, settles it.
/// </summary>
/// <param name="Level">Whom the audience is granted to.</param>
/// <param name="UserId">The user, for <see cref="AudienceLevel.User"/>; empty otherwise.</param>
/// <param name="Audience">The audience that is not declared.</param>
[ReadModel]
public record UndeclaredGrant(AudienceLevel Level, UserId UserId, AudienceName Audience)
{
    /// <summary>
    /// Gets the grants that apply to the tenant of the request, to its users and to the system, whose audience the application
    /// does not declare. Nothing is reported while the application declares no audiences
    /// (<see cref="GuestListOptions.DeclaredAudiences"/> is not set).
    /// </summary>
    /// <param name="guestList">The audiences the product holds.</param>
    /// <param name="tenants">The tenant of the request.</param>
    /// <param name="options">GuestList's options, which name the declared audiences.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The grants that match no declared audience.</returns>
    [Authorize]
    public static async Task<IEnumerable<UndeclaredGrant>> Undeclared(IGuestList guestList, ITenantIdAccessor tenants, IOptions<GuestListOptions> options, CancellationToken cancellationToken) =>
        options.Value.DeclaredAudiences is { } declared
            ? Among(await guestList.Granted(new TenantId(tenants.Current.Value), cancellationToken), declared)
            : [];

    /// <summary>
    /// Picks the grants whose audience is not declared. Audience names are case-sensitive.
    /// </summary>
    /// <param name="granted">The grants.</param>
    /// <param name="declared">The declared audiences.</param>
    /// <returns>The grants that match no declared audience, in a stable order.</returns>
    public static IEnumerable<UndeclaredGrant> Among(IEnumerable<GrantedAudience> granted, IEnumerable<AudienceName> declared)
    {
        var names = declared.ToHashSet();
        return
        [
            .. granted
                .Where(grant => !names.Contains(grant.Audience))
                .OrderBy(grant => grant.Level)
                .ThenBy(grant => grant.UserId.Value, StringComparer.Ordinal)
                .ThenBy(grant => grant.Audience.Value, StringComparer.Ordinal)
                .Select(grant => new UndeclaredGrant(grant.Level, grant.UserId, grant.Audience))
        ];
    }
}
