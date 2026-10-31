// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Marquee.Authoring.WithdrawAnnouncement;

/// <summary>
/// Withdraws an announcement: it is no longer shown to anyone, whether it is showing now or scheduled for later.
/// </summary>
/// <param name="AnnouncementId">The announcement.</param>
[Command]
[Authorize(Policy = MarqueeAuthoring.Policy)]
public record WithdrawAnnouncement(AnnouncementId AnnouncementId)
{
    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <returns>The <see cref="AnnouncementWithdrawn"/> event.</returns>
    public AnnouncementWithdrawn Handle() => new();
}

/// <summary>
/// Validates <see cref="WithdrawAnnouncement"/>.
/// </summary>
public class WithdrawAnnouncementValidator : CommandValidator<WithdrawAnnouncement>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WithdrawAnnouncementValidator"/> class.
    /// </summary>
    /// <param name="announcement">The announcement with the command's id, <see langword="null"/> when it is not authored.</param>
    public WithdrawAnnouncementValidator(Announcement? announcement) => RuleFor(c => (Guid)c.AnnouncementId).MustBeStanding(announcement);
}
