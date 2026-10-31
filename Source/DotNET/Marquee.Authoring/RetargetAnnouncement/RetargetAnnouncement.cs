// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Marquee.Authoring.RetargetAnnouncement;

/// <summary>
/// Gives an announcement another audience. Retargeting to the audience it already has records nothing.
/// </summary>
/// <param name="AnnouncementId">The announcement.</param>
/// <param name="Audience">Who it is for now.</param>
[Command]
[Authorize(Policy = MarqueeAuthoring.Policy)]
public record RetargetAnnouncement(AnnouncementId AnnouncementId, Audience Audience)
{
    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <param name="announcement">The announcement, as it stands; always there, because the validator rejects a command for one that is not authored.</param>
    /// <returns>The <see cref="AnnouncementRetargeted"/> event, or nothing when the audience is the same.</returns>
    public IEnumerable<AnnouncementRetargeted> Handle(Announcement? announcement)
    {
        if (announcement is not null && !AnnouncementRules.Same(announcement.Audience, Audience))
        {
            yield return new AnnouncementRetargeted(Audience);
        }
    }
}

/// <summary>
/// Validates <see cref="RetargetAnnouncement"/>.
/// </summary>
public class RetargetAnnouncementValidator : CommandValidator<RetargetAnnouncement>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RetargetAnnouncementValidator"/> class.
    /// </summary>
    /// <param name="announcement">The announcement with the command's id, <see langword="null"/> when it is not authored.</param>
    public RetargetAnnouncementValidator(Announcement? announcement)
    {
        RuleFor(c => (Guid)c.AnnouncementId).MustBeStanding(announcement);
        RuleFor(c => c.Audience).MustNameWhoItIsFor();
    }
}
