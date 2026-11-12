// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Choreography.Availability.Audiences;
using Cratis.Chronicle;
using Cratis.Showrunner.GuestList.Availability.for_AudienceChangeWatcher.given;
using Cratis.Showrunner.GuestList.Holding;

namespace Cratis.Showrunner.GuestList.Availability.for_AudienceChangeWatcher;

/// <summary>
/// A decision recorded at any level reaches the engine's subscribers, which is how an open session updates.
/// </summary>
public class when_a_decision_is_recorded : a_watcher
{
    async Task Because()
    {
        await _watcher.RunOnce();
        Changed(new("acme"), GuestListStreams.Tenant.Value);
        Changed(new("acme"), GuestListStreams.User(new("alice")).Value);
        Changed(EventStoreNamespaceName.Default, GuestListStreams.System.Value);
        await Task.Yield();
    }

    [Fact] void should_tell_the_tenant_changed() => _heard.ShouldContain(new AudienceChange(AudienceLevel.Tenant, "acme"));
    [Fact] void should_tell_the_user_changed() => _heard.ShouldContain(new AudienceChange(AudienceLevel.User, "alice"));
    [Fact] void should_tell_the_system_changed() => _heard.ShouldContain(new AudienceChange(AudienceLevel.System, null));
    [Fact] void should_note_when_it_was_received() => _feed.LastReceived.ShouldNotBeNull();
}
#endif
