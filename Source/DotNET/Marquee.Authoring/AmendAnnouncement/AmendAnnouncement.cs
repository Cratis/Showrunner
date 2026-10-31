// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Marquee.Authoring.AmendAnnouncement;

/// <summary>
/// Amends what an announcement says, how important it is and when it is shown. How it is displayed cannot change, and
/// who it is for changes with <c>RetargetAnnouncement</c>. Amending to what the announcement already says records
/// nothing.
/// </summary>
/// <param name="AnnouncementId">The announcement.</param>
/// <param name="Title">Its title, up to 100 characters.</param>
/// <param name="Body">What it says, up to 2,000 characters.</param>
/// <param name="Importance">How important it is.</param>
/// <param name="ActiveFrom">When it starts to be shown; the default date means immediately.</param>
/// <param name="ActiveUntil">When it stops being shown; the default date means indefinitely.</param>
[Command]
[Authorize(Policy = MarqueeAuthoring.Policy)]
public record AmendAnnouncement(
    AnnouncementId AnnouncementId,
    AnnouncementTitle Title,
    AnnouncementBody Body,
    Importance Importance,
    ActiveFrom ActiveFrom,
    ActiveUntil ActiveUntil)
{
    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <param name="announcement">The announcement, as it stands; always there, because the validator rejects a command for one that is not authored.</param>
    /// <returns>The <see cref="AnnouncementAmended"/> event, or nothing when it changes nothing.</returns>
    public IEnumerable<AnnouncementAmended> Handle(Announcement? announcement)
    {
        if (announcement is null)
        {
            yield break;
        }

        var amended = new AnnouncementAmended(Title, Body, Importance, AnnouncementRules.StartOf(ActiveFrom), AnnouncementRules.EndOf(ActiveUntil));
        if (announcement.Title == amended.Title &&
            announcement.Body == amended.Body &&
            announcement.Importance == amended.Importance &&
            announcement.ActiveFrom == amended.ActiveFrom &&
            announcement.ActiveUntil == amended.ActiveUntil)
        {
            yield break;
        }

        yield return amended;
    }
}

/// <summary>
/// Validates <see cref="AmendAnnouncement"/>.
/// </summary>
public class AmendAnnouncementValidator : CommandValidator<AmendAnnouncement>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AmendAnnouncementValidator"/> class.
    /// </summary>
    /// <param name="announcement">The announcement with the command's id, <see langword="null"/> when it is not authored.</param>
    public AmendAnnouncementValidator(Announcement? announcement)
    {
        RuleFor(c => (Guid)c.AnnouncementId).MustBeStanding(announcement);
        RuleFor(c => (string)c.Title).MustBeTitled();
        RuleFor(c => (string)c.Body).MustFit();
        RuleFor(c => c).MustStartBeforeItEnds(c => c.ActiveFrom, c => c.ActiveUntil);
        RuleFor(c => c.Importance).IsInEnum();
    }
}
