// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Access.OnBehalf.for_OnBehalfAssertionClaims;

public class when_reading_a_verified_forward : Specification
{
    static readonly DateTimeOffset _now = new(2026, 10, 26, 12, 0, 0, TimeSpan.Zero);
    static readonly OnBehalfSessionFacts _session = new("a-session", "tenant-one", "an-issue", "reporter", "support-person", "aad");

    AuthProxyTokens _authProxy;
    OnBehalfAssertion? _forward;
    OnBehalfAssertion? _call;
    OnBehalfAssertion? _notAToken;

    void Establish() => _authProxy = new();

    void Because()
    {
        _forward = OnBehalfAssertionClaims.FromVerifiedForward(_authProxy.ForForward(_session, "principal", "actor", _now));
        _call = OnBehalfAssertionClaims.FromVerifiedForward(_authProxy.ForCall(OnBehalfPaths.GrantCheckPurpose, HttpMethod.Get, new Uri("https://portal.internal/.cratis/on-behalf/grants"), [], _session, _now));
        _notAToken = OnBehalfAssertionClaims.FromVerifiedForward("not.a-token");
    }

    void Destroy() => _authProxy.Dispose();

    [Fact] void should_read_the_session() => _forward.ShouldEqual(new OnBehalfAssertion("reporter", "support-person", "aad", "tenant-one", "an-issue", "a-session"));
    [Fact] void should_not_read_a_token_for_a_call() => _call.ShouldBeNull();
    [Fact] void should_not_read_something_that_is_not_a_token() => _notAToken.ShouldBeNull();
}
#endif
