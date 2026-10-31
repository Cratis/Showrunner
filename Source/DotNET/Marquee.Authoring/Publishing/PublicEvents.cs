// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Marquee.Authoring.Publishing;

/// <summary>
/// Turns what was authored into the public events a product receives. Each method is one row of the mapping table in
/// the authoring documentation.
/// </summary>
public static class PublicEvents
{
    /// <summary>
    /// Maps an announcement being authored.
    /// </summary>
    /// <param name="announcementId">The announcement.</param>
    /// <param name="event">The private event.</param>
    /// <returns>The public event.</returns>
    public static AnnouncementPublished For(AnnouncementId announcementId, AnnouncementAuthored @event) => new(
        announcementId,
        @event.Product,
        @event.Audience,
        @event.Title,
        @event.Body,
        @event.Display,
        @event.Importance,
        @event.ActiveFrom,
        @event.ActiveUntil);

    /// <summary>
    /// Maps an announcement being amended.
    /// </summary>
    /// <param name="announcementId">The announcement.</param>
    /// <param name="product">The product it is for.</param>
    /// <param name="event">The private event.</param>
    /// <returns>The public event.</returns>
    public static AnnouncementRevised For(AnnouncementId announcementId, Product product, AnnouncementAmended @event) => new(
        announcementId,
        product,
        @event.Title,
        @event.Body,
        @event.Importance,
        @event.ActiveFrom,
        @event.ActiveUntil);

    /// <summary>
    /// Maps an announcement being retargeted.
    /// </summary>
    /// <param name="announcementId">The announcement.</param>
    /// <param name="product">The product it is for.</param>
    /// <param name="event">The private event.</param>
    /// <returns>The public event.</returns>
    public static AnnouncementAudienceChanged For(AnnouncementId announcementId, Product product, AnnouncementRetargeted @event) =>
        new(announcementId, product, @event.Audience);

    /// <summary>
    /// Maps an announcement being withdrawn.
    /// </summary>
    /// <param name="announcementId">The announcement.</param>
    /// <param name="product">The product it is for.</param>
    /// <returns>The public event.</returns>
    public static AnnouncementRetracted For(AnnouncementId announcementId, Product product) => new(announcementId, product);
}
