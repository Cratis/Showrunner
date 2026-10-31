// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Marquee.Viewing;

/// <summary>
/// Chooses, out of the announcements the product holds, the ones a person in a tenant is to see.
/// </summary>
static class AnnouncementsForTheTenant
{
    /// <summary>
    /// Chooses the announcements for a person: those whose audience includes the tenant, leaving out the dialogs shown
    /// once that the person already acknowledged. They come in the order they start to be shown.
    /// </summary>
    /// <param name="held">The announcements the product holds.</param>
    /// <param name="acknowledged">Who acknowledged what in the tenant.</param>
    /// <param name="tenant">The tenant.</param>
    /// <param name="person">The person.</param>
    /// <param name="entitlements">What answers module and plan membership.</param>
    /// <returns>The announcements for the person.</returns>
    internal static async Task<IEnumerable<AnnouncementForMe>> Select(
        IEnumerable<StandingAnnouncement> held,
        IEnumerable<AnnouncementAcknowledgements> acknowledged,
        Contracts.TenantId tenant,
        UserSubject person,
        IAudienceEntitlements entitlements)
    {
        var seen = acknowledged.Where(acknowledgements => acknowledgements.WasAcknowledgedBy(person)).Select(acknowledgements => acknowledgements.Id).ToHashSet();
        List<AnnouncementForMe> result = [];
        foreach (var announcement in held.OrderBy(announcement => announcement.ActiveFrom.Value).ThenBy(announcement => announcement.Id.Value))
        {
            if (announcement.Display == Display.DialogOnce && seen.Contains(announcement.Id))
            {
                continue;
            }

            if (await announcement.Audience.Includes(tenant, entitlements))
            {
                result.Add(new AnnouncementForMe(announcement.Id, announcement.Title, announcement.Body, announcement.Display, announcement.Importance, announcement.ActiveFrom, announcement.ActiveUntil));
            }
        }

        return result;
    }
}
