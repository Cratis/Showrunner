// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Choreography.Availability.Audiences;
using Cratis.Showrunner.GuestList.Availability.given;

namespace Cratis.Showrunner.GuestList.Availability.for_GuestListAudienceSource;

/// <summary>
/// An open session that subscribed through the source hears about a change, until it disposes the subscription, and one
/// subscriber failing does not keep another from hearing.
/// </summary>
public class when_a_change_is_published : a_source_over_a_guest_list
{
    readonly List<AudienceChange> _heard = [];
    readonly List<AudienceChange> _heardAfterDisposal = [];
    IDisposable _disposed;

    void Establish()
    {
        _source.OnChanged(_ => throw new InvalidOperationException("A subscriber that fails."));
        _source.OnChanged(change =>
        {
            _heard.Add(change);
            return ValueTask.CompletedTask;
        });
        _disposed = _source.OnChanged(change =>
        {
            _heardAfterDisposal.Add(change);
            return ValueTask.CompletedTask;
        });
        _disposed.Dispose();
    }

    async Task Because()
    {
        await _feed.Publish(new(AudienceLevel.Tenant, "acme"));
        await _feed.Publish(new(AudienceLevel.System, null));
    }

    [Fact] void should_tell_the_subscriber_every_change() => _heard.ShouldContainOnly(new AudienceChange(AudienceLevel.Tenant, "acme"), new AudienceChange(AudienceLevel.System, null));
    [Fact] void should_not_tell_a_subscriber_that_disposed() => _heardAfterDisposal.ShouldBeEmpty();
}
#endif
