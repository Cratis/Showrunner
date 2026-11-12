// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Choreography.Availability;
using Cratis.Choreography.Availability.Audiences;
using Cratis.Showrunner.GuestList.Availability.given;

namespace Cratis.Showrunner.GuestList.Availability.for_GuestListAudienceSource;

/// <summary>
/// A grant for an audience the deployed model does not declare is carried, never matches a gate and is reported.
/// </summary>
public class when_an_audience_is_granted_that_is_not_declared : a_source_over_a_guest_list
{
    IUndeclaredAudienceObserver _observer;
    EffectiveAudiences _effective;

    void Establish()
    {
        Holds(system: [], tenant: ["Plus", "Beta"], user: []);
        _observer = Substitute.For<IUndeclaredAudienceObserver>();
    }

    async Task Because() =>
        _effective = await new AudienceResolver(_source, new DeclaredAudiences("Plus"), new AvailabilityOptions(), _observer).Resolve(new("acme", "alice"));

    [Fact] void should_match_the_declared_audience() => _effective.Holds("Plus").ShouldBeTrue();
    [Fact] void should_not_match_the_undeclared_audience() => _effective.Holds("Beta").ShouldBeFalse();
    [Fact] void should_carry_the_undeclared_grant() => _effective.Undeclared.ShouldContainOnly(new AudienceGrant("Beta", AudienceLevel.Tenant));
    [Fact] void should_report_it() => _observer.Received(1).Observe(new("acme", "alice"), Arg.Is<IReadOnlyCollection<AudienceGrant>>(_ => _.Count == 1));
}
#endif
