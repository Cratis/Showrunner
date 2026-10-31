// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Marquee.Authoring.AuthorAnnouncement;

/// <summary>
/// Authors an announcement for a product.
/// </summary>
/// <remarks>
/// No window means the announcement is shown immediately and indefinitely: send <see cref="Contracts.Announcements.ActiveFrom.Immediately"/>
/// and <see cref="Contracts.Announcements.ActiveUntil.Indefinitely"/>, or the default date of 0001-01-01 for either. The window is exact UTC times, so a maintenance window can be announced to the
/// minute. The id is chosen by the caller, so a retried command cannot author a second announcement.
/// </remarks>
/// <param name="AnnouncementId">The new announcement; create it with <c>AnnouncementId.New()</c>.</param>
/// <param name="Product">The product the announcement is for.</param>
/// <param name="Audience">Who it is for.</param>
/// <param name="Title">Its title, up to 100 characters.</param>
/// <param name="Body">What it says, up to 2,000 characters.</param>
/// <param name="Display">How it is shown; it cannot change afterwards.</param>
/// <param name="Importance">How important it is.</param>
/// <param name="ActiveFrom">When it starts to be shown; the default date means immediately.</param>
/// <param name="ActiveUntil">When it stops being shown; the default date means indefinitely.</param>
[Command]
[Authorize(Policy = MarqueeAuthoring.Policy)]
public record AuthorAnnouncement(
    AnnouncementId AnnouncementId,
    Product Product,
    Audience Audience,
    AnnouncementTitle Title,
    AnnouncementBody Body,
    Display Display,
    Importance Importance,
    ActiveFrom ActiveFrom,
    ActiveUntil ActiveUntil)
{
    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <returns>The <see cref="AnnouncementAuthored"/> event.</returns>
    public AnnouncementAuthored Handle() => new(
        Product,
        Audience,
        Title,
        Body,
        Display,
        Importance,
        AnnouncementRules.StartOf(ActiveFrom),
        AnnouncementRules.EndOf(ActiveUntil));
}

/// <summary>
/// Validates <see cref="AuthorAnnouncement"/>.
/// </summary>
public class AuthorAnnouncementValidator : CommandValidator<AuthorAnnouncement>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorAnnouncementValidator"/> class.
    /// </summary>
    /// <param name="announcement">The announcement with the command's id, <see langword="null"/> when it is not authored yet.</param>
    public AuthorAnnouncementValidator(Announcement? announcement)
    {
        RuleFor(c => (Guid)c.AnnouncementId).MustNotBeAuthored(announcement);
        RuleFor(c => (string)c.Product).NotEmpty().WithMessage("Say which product the announcement is for.");
        RuleFor(c => c.Audience).MustNameWhoItIsFor();
        RuleFor(c => (string)c.Title).MustBeTitled();
        RuleFor(c => (string)c.Body).MustFit();
        RuleFor(c => c).MustStartBeforeItEnds(c => c.ActiveFrom, c => c.ActiveUntil);
        RuleFor(c => c.Display).IsInEnum();
        RuleFor(c => c.Importance).IsInEnum();
    }
}
