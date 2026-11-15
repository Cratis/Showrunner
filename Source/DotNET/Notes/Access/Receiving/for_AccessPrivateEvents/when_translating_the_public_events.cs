// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Access.given;
using Cratis.Showrunner.Notes.Contracts.Access;

namespace Cratis.Showrunner.Notes.Access.Receiving.for_AccessPrivateEvents;

/// <summary>
/// Each public event a desk sends about access has one private fact, for the user and tenant it names.
/// </summary>
public class when_translating_the_public_events : Specification
{
    (TenantId TenantId, FeedbackGiverIdentity Giver, object Fact)? _granted;
    (TenantId TenantId, FeedbackGiverIdentity Giver, object Fact)? _revoked;
    (TenantId TenantId, FeedbackGiverIdentity Giver, object Fact)? _other;

    void Because()
    {
        _granted = AccessPrivateEvents.For(AccessEvents.PublicGranted());
        _revoked = AccessPrivateEvents.For(AccessEvents.PublicRevoked());
        _other = AccessPrivateEvents.For(new NoteAcknowledged(AccessEvents.Tenant));
    }

    [Fact] void should_translate_a_grant() => _granted!.Value.Fact.ShouldEqual(new DeskGrantedFeedbackAccess(AccessEvents.Grantor));
    [Fact] void should_translate_a_revocation() => _revoked!.Value.Fact.ShouldEqual(new DeskRevokedFeedbackAccess(AccessEvents.Grantor));
    [Fact] void should_name_the_tenant() => _granted!.Value.TenantId.ShouldEqual(AccessEvents.Tenant);
    [Fact] void should_name_the_user() => _revoked!.Value.Giver.ShouldEqual(AccessEvents.Giver);
    [Fact] void should_translate_nothing_else() => _other.ShouldBeNull();
    [Fact] void should_subscribe_to_exactly_the_two_decisions() => AccessPrivateEvents.FromDesk.ShouldContainOnly(typeof(FeedbackAccessGranted), typeof(FeedbackAccessRevoked));
}
#endif
