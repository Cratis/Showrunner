// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.GuestList.Availability.for_AudienceChangeWatcher.given;

namespace Cratis.Showrunner.GuestList.Availability.for_AudienceChangeWatcher;

/// <summary>
/// Each round watches what is not watched: a tenant that appears is picked up, one already watched is not watched twice, and
/// a watch that failed is opened again.
/// </summary>
public class when_running_rounds : a_watcher
{
    int _acmeAfterFirst;
    int _acmeAfterSecond;
    int _acmeAfterFailure;
    int _globexAfterItAppears;

    async Task Because()
    {
        await _watcher.RunOnce();
        _acmeAfterFirst = _watched[new("acme")].Subscriptions;
        await _watcher.RunOnce();
        _acmeAfterSecond = _watched[new("acme")].Subscriptions;

        _tenants.Add(new("globex"));
        await _watcher.RunOnce();
        _globexAfterItAppears = _watched[new("globex")].Subscriptions;

        _watched[new("acme")].Fail();
        await _watcher.RunOnce();
        _acmeAfterFailure = _watched[new("acme")].Subscriptions;
    }

    [Fact] void should_watch_a_tenant() => _acmeAfterFirst.ShouldEqual(1);
    [Fact] void should_watch_the_product_namespace_for_the_system() => _watched.ContainsKey(EventStoreNamespaceName.Default).ShouldBeTrue();
    [Fact] void should_not_watch_the_same_namespace_twice() => _acmeAfterSecond.ShouldEqual(1);
    [Fact] void should_watch_a_tenant_that_appears_later() => _globexAfterItAppears.ShouldEqual(1);
    [Fact] void should_watch_again_after_a_watch_fails() => _acmeAfterFailure.ShouldEqual(2);
}
#endif
