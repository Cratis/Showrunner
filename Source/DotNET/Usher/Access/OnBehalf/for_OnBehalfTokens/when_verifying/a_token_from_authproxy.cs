// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access.OnBehalf.for_OnBehalfTokens.given;
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Access.OnBehalf.for_OnBehalfTokens.when_verifying;

public class a_token_from_authproxy : on_behalf_tokens
{
    OnBehalfAssertion _assertion;

    async Task Because()
    {
        await Verify(GrantCheckToken());
        _result.TryGetResult(out _assertion);
    }

    [Fact] void should_accept_it() => _result.IsSuccess.ShouldBeTrue();
    [Fact] void should_take_the_represented_user_from_it() => _assertion.Subject.ShouldEqual(Reporters.Subject);
    [Fact] void should_take_the_actor_from_it() => ((string)_assertion.Actor).ShouldEqual(Session.Actor);
    [Fact] void should_take_the_actors_identity_provider_from_it() => ((string)_assertion.ActorIdentityProvider).ShouldEqual(Session.ActorIdentityProvider);
    [Fact] void should_take_the_tenant_from_it() => ((string)_assertion.Tenant).ShouldEqual(Session.Tenant);
    [Fact] void should_take_the_issue_from_it() => _assertion.Issue.ShouldEqual(Session.Issue);
    [Fact] void should_take_the_session_from_it() => ((string)_assertion.SessionId).ShouldEqual(Session.SessionId);
}
#endif
