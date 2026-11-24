// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Arc.Availability.Audiences;
using Cratis.Arc.Availability.Suspensions;
using Cratis.Showrunner.GuestList.Availability.given;
using Cratis.Showrunner.GuestList.Querying;
using Microsoft.Extensions.Logging.Abstractions;

namespace Cratis.Showrunner.GuestList.Availability.for_GuestListSuspensionSource;

/// <summary>
/// An open session that subscribed through the source hears about a suspension or a resume until it disposes the
/// subscription, one subscriber failing does not keep another from hearing, and an audience change is not a suspension
/// change.
/// </summary>
public class when_a_suspension_changes : Specification
{
    readonly List<SuspensionChange> _heard = [];
    readonly List<SuspensionChange> _heardAfterDisposal = [];
    readonly List<AudienceChange> _heardAudiences = [];
    AudienceChangeFeed _feed;
    GuestListSuspensionSource _source;

    void Establish()
    {
        _feed = new(new a_clock(), NullLogger<AudienceChangeFeed>.Instance);
        _source = new(Substitute.For<IPartSuspensions>(), _feed);
        _source.OnChanged(_ => throw new InvalidOperationException("A subscriber that fails."));
        _source.OnChanged(change =>
        {
            _heard.Add(change);
            return ValueTask.CompletedTask;
        });
        _source.OnChanged(change =>
        {
            _heardAfterDisposal.Add(change);
            return ValueTask.CompletedTask;
        }).Dispose();
        _feed.Subscribe(change =>
        {
            _heardAudiences.Add(change);
            return ValueTask.CompletedTask;
        });
    }

    async Task Because()
    {
        await _feed.PublishSuspension(new(AudienceLevel.Tenant, "acme"));
        await _feed.PublishSuspension(new(AudienceLevel.System, null));
    }

    [Fact] void should_tell_the_subscriber_about_each_change() => _heard.Select(_ => _.Level).ShouldEqual([AudienceLevel.Tenant, AudienceLevel.System]);
    [Fact] void should_say_which_tenant_changed() => _heard[0].Key.ShouldEqual("acme");
    [Fact] void should_not_tell_a_subscriber_that_stopped_listening() => _heardAfterDisposal.ShouldBeEmpty();
    [Fact] void should_not_tell_an_audience_subscriber() => _heardAudiences.ShouldBeEmpty();
}
#endif
