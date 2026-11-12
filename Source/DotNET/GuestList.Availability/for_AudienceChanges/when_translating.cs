// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Choreography.Availability.Audiences;
using Cratis.Chronicle;
using Cratis.Showrunner.GuestList.Holding;

namespace Cratis.Showrunner.GuestList.Availability.for_AudienceChanges;

/// <summary>
/// A change to a holding means a change at its level: the tenant's is keyed by the namespace it lives in, the user's by the
/// user, and the system's by nothing.
/// </summary>
public class when_translating : Specification
{
    AudienceChange? _tenant;
    AudienceChange? _user;
    AudienceChange? _system;
    AudienceChange? _unknown;
    AudienceChange? _emptyUser;

    void Because()
    {
        _tenant = AudienceChanges.From(new("acme"), GuestListStreams.Tenant.Value);
        _user = AudienceChanges.From(new("acme"), GuestListStreams.User(new("alice")).Value);
        _system = AudienceChanges.From(EventStoreNamespaceName.Default, GuestListStreams.System.Value);
        _unknown = AudienceChanges.From(new("acme"), "something-else");
        _emptyUser = AudienceChanges.From(new("acme"), "user:");
    }

    [Fact] void should_key_the_tenant_by_its_namespace() => _tenant.ShouldEqual(new AudienceChange(AudienceLevel.Tenant, "acme"));
    [Fact] void should_key_the_user_by_the_user() => _user.ShouldEqual(new AudienceChange(AudienceLevel.User, "alice"));
    [Fact] void should_key_the_system_by_nothing() => _system.ShouldEqual(new AudienceChange(AudienceLevel.System, null));
    [Fact] void should_ignore_a_stream_that_is_not_guest_lists() => _unknown.ShouldBeNull();
    [Fact] void should_ignore_a_user_without_a_name() => _emptyUser.ShouldBeNull();
}
#endif
