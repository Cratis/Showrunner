// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Choreography.Availability;
using Cratis.Choreography.Availability.Audiences;
using Cratis.Showrunner.GuestList.Availability.given;

namespace Cratis.Showrunner.GuestList.Availability.for_GuestListAudienceSource;

/// <summary>
/// Withdrawing an audience from the tenant removes it at that level only: a user who holds it as well stays in it.
/// </summary>
public class when_a_level_is_withdrawn : a_source_over_a_guest_list
{
    AudienceResolver _resolver;
    EffectiveAudiences _before;
    EffectiveAudiences _after;
    EffectiveAudiences _afterBoth;

    void Establish() => _resolver = new(_source, new DeclaredAudiences("Plus"), new AvailabilityOptions());

    async Task Because()
    {
        Holds(system: [], tenant: ["Plus"], user: ["Plus"]);
        _before = await _resolver.Resolve(new("acme", "alice"));
        Holds(system: [], tenant: [], user: ["Plus"]);
        _after = await _resolver.Resolve(new("acme", "alice"));
        Holds(system: [], tenant: [], user: []);
        _afterBoth = await _resolver.Resolve(new("acme", "alice"));
    }

    [Fact] void should_hold_the_audience_before() => _before.Holds("Plus").ShouldBeTrue();
    [Fact] void should_keep_the_user_in_the_audience_after() => _after.Holds("Plus").ShouldBeTrue();
    [Fact] void should_leave_the_audience_when_every_level_has_withdrawn_it() => _afterBoth.Holds("Plus").ShouldBeFalse();
}
#endif
