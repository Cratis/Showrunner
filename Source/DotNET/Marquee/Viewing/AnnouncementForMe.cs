// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Cratis.Arc.Queries.ModelBound;
using Cratis.Chronicle.ReadModels;
using ReactiveSubject = System.Reactive.Subjects.Subject;

namespace Cratis.Showrunner.Marquee.Viewing;

/// <summary>
/// An announcement the signed-in person is to see: the product holds it, it is for their tenant, and it is not a dialog
/// they have already acknowledged.
/// </summary>
/// <param name="Id">The announcement.</param>
/// <param name="Title">Its title.</param>
/// <param name="Body">What it says.</param>
/// <param name="Display">How it is shown.</param>
/// <param name="Importance">How important it is.</param>
/// <param name="ActiveFrom">When it starts to be shown.</param>
/// <param name="ActiveUntil">When it stops being shown.</param>
[ReadModel]
public record AnnouncementForMe(
    AnnouncementId Id,
    AnnouncementTitle Title,
    AnnouncementBody Body,
    Display Display,
    Importance Importance,
    ActiveFrom ActiveFrom,
    ActiveUntil ActiveUntil)
{
    /// <summary>
    /// Observes the announcements the signed-in person is to see.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The announcements are the product's, the same for every tenant; the query keeps those whose audience includes the
    /// tenant of the request. A tenant provisioned after an announcement was published sees it, since nothing is copied
    /// to a tenant when an announcement arrives. <see cref="AudienceKind.HoldersOfModule"/> and
    /// <see cref="AudienceKind.OnPlan"/> are asked of the host's <see cref="IAudienceEntitlements"/>, which Box Office
    /// supplies when it is present; without one they never match.
    /// </para>
    /// <para>
    /// A dialog shown once that the person acknowledged in this tenant is left out; it is still there for everyone else.
    /// Nothing is filtered by date: an announcement scheduled for later is returned, and the client checks the window of
    /// each every 60 seconds, so it shows up on time without a round trip.
    /// </para>
    /// </remarks>
    /// <param name="readModels">The <see cref="IReadModels"/> of the current tenant's event store namespace.</param>
    /// <param name="standing">The announcements the product holds.</param>
    /// <param name="tenant">The tenant of the request.</param>
    /// <param name="person">The signed-in person.</param>
    /// <param name="entitlements">What answers module and plan membership.</param>
    /// <returns>The announcements for the person, again every time something changes.</returns>
    [Authorize]
    public static ISubject<IEnumerable<AnnouncementForMe>> AnnouncementsForMe(
        IReadModels readModels,
        IStandingAnnouncements standing,
        ICurrentTenant tenant,
        ISignedInPerson person,
        IAudienceEntitlements entitlements)
    {
        if (!person.IsSignedIn)
        {
            return new BehaviorSubject<IEnumerable<AnnouncementForMe>>([]);
        }

        var announcements = standing.Observe();
        var acknowledgements = readModels.Observe<AnnouncementAcknowledgements>();
        var observed = announcements
            .CombineLatest(acknowledgements, (held, acknowledged) => (held, acknowledged))
            .Select(state => Observable.FromAsync(() => AnnouncementsForTheTenant.Select(state.held, state.acknowledged, tenant.Id, person.Subject, entitlements)))
            .Switch();

        return ReactiveSubject.Create<IEnumerable<AnnouncementForMe>>(
            Observer.Create<IEnumerable<AnnouncementForMe>>(_ => { }, announcements.OnError, announcements.OnCompleted),
            observed);
    }
}
