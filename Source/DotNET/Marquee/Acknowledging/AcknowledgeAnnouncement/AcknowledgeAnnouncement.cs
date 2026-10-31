// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Arc.Validation;
using Cratis.Chronicle.EventSequences;
using Cratis.Monads;

namespace Cratis.Showrunner.Marquee.Acknowledging.AcknowledgeAnnouncement;

/// <summary>
/// Records that the signed-in person saw an announcement shown in a dialog once, so it is not shown to them again on any
/// device.
/// </summary>
/// <remarks>
/// <para>
/// Acknowledging twice appends nothing. An announcement shown in a dialog every session records nothing either: looking
/// is not a fact, and it comes back at the next session whatever the person did. A banner can not be dismissed, so it
/// can not be acknowledged. An announcement the product does not hold, or that is not for the person's tenant, is
/// refused.
/// </para>
/// <para>
/// The acknowledgement is appended in the tenant's namespace, under the announcement's id, with the person as the
/// compliance subject.
/// </para>
/// </remarks>
/// <param name="AnnouncementId">The announcement.</param>
[Command]
[Authorize]
public record AcknowledgeAnnouncement(AnnouncementId AnnouncementId)
{
    /// <summary>
    /// Finds the announcement and checks it can be acknowledged by the signed-in person.
    /// </summary>
    /// <param name="standing">The announcements the product holds.</param>
    /// <param name="tenant">The tenant of the request.</param>
    /// <param name="person">The signed-in person.</param>
    /// <param name="entitlements">What answers module and plan membership.</param>
    /// <returns>The announcement, or why it can not be acknowledged.</returns>
    public async Task<Result<StandingAnnouncement, ValidationResult>> Provide(
        IStandingAnnouncements standing,
        ICurrentTenant tenant,
        ISignedInPerson person,
        IAudienceEntitlements entitlements)
    {
        if (!person.IsSignedIn)
        {
            return ValidationResult.Error("Sign in to acknowledge an announcement.");
        }

        var announcement = await standing.Find(AnnouncementId);
        if (announcement is null)
        {
            return ValidationResult.Error("This announcement is not available.");
        }

        if (!await announcement.Audience.Includes(tenant.Id, entitlements))
        {
            return ValidationResult.Error("This announcement is not for you.");
        }

        if (announcement.Display == Display.Banner)
        {
            return ValidationResult.Error("A banner can not be dismissed.");
        }

        return announcement;
    }

    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <param name="announcement">The announcement.</param>
    /// <param name="acknowledgements">Who in the tenant acknowledged the announcement already; <see langword="null"/> when no one did.</param>
    /// <param name="person">The signed-in person.</param>
    /// <returns>The <see cref="AnnouncementAcknowledged"/> event, or nothing when it is not a fact to record.</returns>
    public IEnumerable<EventForEventSourceId> Handle(StandingAnnouncement announcement, AnnouncementAcknowledgements? acknowledgements, ISignedInPerson person)
    {
        if (announcement.Display != Display.DialogOnce || acknowledgements?.WasAcknowledgedBy(person.Subject) == true)
        {
            yield break;
        }

        yield return new EventForEventSourceId(AnnouncementId, new AnnouncementAcknowledged(AnnouncementId, person.Subject))
        {
            Subject = PersonSubjects.For(person.Subject)
        };
    }
}
