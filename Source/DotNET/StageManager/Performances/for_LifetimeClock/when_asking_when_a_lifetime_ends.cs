// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_LifetimeClock;

public class when_asking_when_a_lifetime_ends : Specification
{
    static readonly DateTimeOffset _now = new(2026, 11, 1, 12, 0, 0, TimeSpan.Zero);

    PerformanceSpec Spec(LifetimeSpec? lifetime)
    {
        var spec = PerformanceSamples.Performance().Spec;
        spec.Lifetime = lifetime;
        return spec;
    }

    [Fact] void should_never_expire_without_a_lifetime() => LifetimeClock.IsExpired(Spec(null), _now).ShouldBeFalse();
    [Fact] void should_never_expire_when_permanent() => LifetimeClock.IsExpired(Spec(new() { Permanent = true }), _now.AddYears(50)).ShouldBeFalse();
    [Fact] void should_have_no_expiry_when_permanent() => LifetimeClock.ExpiresAt(Spec(new() { Permanent = true })).ShouldBeNull();
    [Fact] void should_not_be_expired_before_the_time() => LifetimeClock.IsExpired(Spec(new() { Until = _now.AddSeconds(1) }), _now).ShouldBeFalse();
    [Fact] void should_be_expired_at_the_time() => LifetimeClock.IsExpired(Spec(new() { Until = _now }), _now).ShouldBeTrue();
    [Fact] void should_be_expired_after_the_time() => LifetimeClock.IsExpired(Spec(new() { Until = _now.AddDays(-1) }), _now).ShouldBeTrue();
}
#endif
