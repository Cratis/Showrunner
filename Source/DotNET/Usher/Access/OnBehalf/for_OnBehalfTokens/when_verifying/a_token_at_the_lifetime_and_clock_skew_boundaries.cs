// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access.OnBehalf.for_OnBehalfTokens.given;

namespace Cratis.Showrunner.Usher.Access.OnBehalf.for_OnBehalfTokens.when_verifying;

public class a_token_at_the_lifetime_and_clock_skew_boundaries : on_behalf_tokens
{
    Task Because() => Verify(_authProxy.ForCall(
        OnBehalfPaths.GrantCheckPurpose, HttpMethod.Get, Target, [], Session, Now.AddSeconds(5), lifetime: OnBehalfTokens.MaximumLifetime));

    [Fact] void should_accept_exactly_the_maximum_lifetime_and_five_seconds_of_skew() => _result.IsSuccess.ShouldBeTrue();
}
#endif
