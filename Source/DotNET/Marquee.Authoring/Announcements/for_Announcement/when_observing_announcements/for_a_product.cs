// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Arc.Queries;
using Cratis.Showrunner.Marquee.Authoring.given;

namespace Cratis.Showrunner.Marquee.Authoring.Announcements.for_Announcement.when_observing_announcements;

/// <summary>
/// The authoring screen of a product shows its announcements, withdrawn ones included, and nobody else's.
/// </summary>
public class for_a_product : an_authoring_query
{
    AnnouncementId _withdrawn;
    AnnouncementId _standing;

    void Establish()
    {
        _standing = AnnouncementId.New();
        _withdrawn = AnnouncementId.New();
        Given(_standing, AnnouncementEvents.Maintenance);
        Given(_withdrawn, AnnouncementEvents.NewModule, new AnnouncementWithdrawn());
        Given(AnnouncementId.New(), AnnouncementEvents.Then(AnnouncementEvents.Maintenance with { Product = new Product("studio") }));
    }

    async Task Because() => _result = await _scenario.Perform(nameof(Announcement.ForProduct), new QueryArguments { ["product"] = "ledger" });

    [Fact] void should_succeed() => _result.IsSuccess.ShouldBeTrue();

    [Fact]
    void should_return_the_announcements_of_the_product_newest_window_first() =>
        ((IEnumerable<Announcement>)_result.Data).Select(announcement => announcement.Id).ShouldEqual([_standing, _withdrawn]);

    [Fact]
    void should_return_the_withdrawn_one_as_withdrawn() =>
        ((IEnumerable<Announcement>)_result.Data).Single(announcement => announcement.Id == _withdrawn).Withdrawn.ShouldBeTrue();
}
#endif
