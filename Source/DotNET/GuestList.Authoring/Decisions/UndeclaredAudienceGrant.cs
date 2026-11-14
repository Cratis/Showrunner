// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive;
using System.Reactive.Linq;
using Cratis.Arc.Queries.ModelBound;
using Cratis.Chronicle.ReadModels;
using Cratis.Showrunner.GuestList.Authoring.Availability;
using Microsoft.Extensions.Options;
using ReactiveSubject = System.Reactive.Subjects.Subject;

namespace Cratis.Showrunner.GuestList.Authoring.Decisions;

/// <summary>
/// A grant of an audience the product's application does not declare: the "granted but not declared" report. The grant is
/// kept, because a decision is a fact, but it never matches a gate. Declaring the audience, or withdrawing the grant,
/// settles it.
/// </summary>
/// <param name="Level">Whom the audience is granted to.</param>
/// <param name="TenantId">The tenant, for <see cref="AudienceLevel.Tenant"/> and <see cref="AudienceLevel.User"/>; empty otherwise.</param>
/// <param name="UserId">The user, for <see cref="AudienceLevel.User"/>; empty otherwise.</param>
/// <param name="Audience">The audience that is not declared.</param>
/// <param name="ClientId">The client id of the system, for <see cref="AudienceLevel.NamedSystem"/>; empty otherwise.</param>
/// <param name="Kind">The kind of the system, for <see cref="AudienceLevel.NamedSystem"/>; none otherwise.</param>
[ReadModel]
public record UndeclaredAudienceGrant(AudienceLevel Level, TenantId TenantId, UserId UserId, AudienceName Audience, SystemClientId ClientId, SystemKind Kind)
{
    /// <summary>
    /// Observes the grants of a product whose audience its application does not declare. Nothing is reported for a product
    /// whose declared audiences are not in <see cref="GuestListAuthoringOptions.DeclaredAudiences"/>: without a declaration
    /// to compare with, no grant can be called undeclared.
    /// </summary>
    /// <param name="product">The product.</param>
    /// <param name="readModels">The <see cref="IReadModels"/> of the current event store namespace.</param>
    /// <param name="options">The options, which hold the declared audiences of each product.</param>
    /// <returns>The grants that match no declared audience, again every time the guest list changes.</returns>
    [Authorize(Policy = GuestListAuthoring.Policy)]
    public static System.Reactive.Subjects.ISubject<IEnumerable<UndeclaredAudienceGrant>> ForProduct(Product product, IReadModels readModels, IOptions<GuestListAuthoringOptions> options)
    {
        var observed = readModels.Observe<ProductGuestList>(list => list.Product == product);
        var reported = readModels.Observe<ProductAvailability>(availability => availability.Product == product);
        var report = observed.CombineLatest(
            reported,
            (lists, availabilities) => DeclaredAudiencesOf.Product(product, availabilities, options.Value) is { } declared
                ? Among(lists.SelectMany(list => list.Grants), declared)
                : []);
        return ReactiveSubject.Create<IEnumerable<UndeclaredAudienceGrant>>(Observer.Create<IEnumerable<UndeclaredAudienceGrant>>(_ => { }, observed.OnError, observed.OnCompleted), report);
    }

    /// <summary>
    /// Picks the grants whose audience is not declared. Audience names are case-sensitive.
    /// </summary>
    /// <param name="grants">The grants held.</param>
    /// <param name="declared">The declared audiences.</param>
    /// <returns>The grants that match no declared audience.</returns>
    public static IEnumerable<UndeclaredAudienceGrant> Among(IEnumerable<HeldAudience> grants, IEnumerable<AudienceName> declared)
    {
        var names = declared.ToHashSet();
        return [.. grants.Where(grant => !names.Contains(grant.Audience)).Select(grant => new UndeclaredAudienceGrant(grant.Level, grant.TenantId, grant.UserId, grant.Audience, grant.ClientId ?? SystemClientId.NotSet, grant.Kind))];
    }
}
