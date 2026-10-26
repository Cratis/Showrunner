// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access.GrantAccess;
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Identity.ShareIdentity;

namespace Cratis.Showrunner.Usher.Access.Sessions.for_SupportSessions.when_checking_a_grant;

/// <summary>
/// The identity shared on the issue names the reporter's subject at another identity provider: that is somebody else,
/// so support has nobody to act as.
/// </summary>
public class and_the_shared_identity_is_at_another_provider : a_refusal
{
    async Task Because()
    {
        await Happened(
            new ReporterSharedIdentity(Reporters.Reporter with { IdentityProvider = Reporters.AnotherIdentityProvider }),
            new ReporterGrantedAccess(ValidUntil));
        _result = await _sessions.Check(_eventStore, Request);
    }

    [Fact] void should_not_be_live() => _result.IsSuccess.ShouldBeFalse();
    [Fact] void should_say_why() => Refusal().ShouldEqual(SupportSessionRefusal.IdentityNotShared);
}
#endif
