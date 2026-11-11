// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access.OnBehalf.for_OnBehalfTokens.given;

namespace Cratis.Showrunner.Usher.Access.OnBehalf.for_OnBehalfTokens.when_verifying;

public class lifetime_boundaries : on_behalf_tokens
{
    OnBehalfTokenRejection _tooLong;
    bool _withinSkew;

    async Task Because()
    {
        await Verify(_authProxy.ForCall(OnBehalfPaths.GrantCheckPurpose, HttpMethod.Get, Target, [], Session, Now, lifetime: OnBehalfTokens.MaximumLifetime.Add(TimeSpan.FromSeconds(1))));
        _tooLong = Rejection();
        await Verify(_authProxy.ForCall(OnBehalfPaths.GrantCheckPurpose, HttpMethod.Get, Target, [], Session, Now.AddMinutes(-1).AddSeconds(-5)));
        _withinSkew = _result.IsSuccess;
        await Verify(_authProxy.ForCall(OnBehalfPaths.GrantCheckPurpose, HttpMethod.Get, Target, [], Session, Now.AddMinutes(-1).AddSeconds(-6)));
    }

    [Fact] void should_refuse_one_second_over_the_maximum() => _tooLong.ShouldEqual(OnBehalfTokenRejection.Expired);
    [Fact] void should_accept_expiration_exactly_at_clock_skew() => _withinSkew.ShouldBeTrue();
    [Fact] void should_refuse_expiration_beyond_clock_skew() => Rejection().ShouldEqual(OnBehalfTokenRejection.Expired);
}
#endif
