// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access.OnBehalf.for_OnBehalfTokens.given;

namespace Cratis.Showrunner.Usher.Access.OnBehalf.for_OnBehalfTokens.when_verifying;

public class a_token_that_is_not_a_token : on_behalf_tokens
{
    Task Because() => Verify("not-a-token");

    [Fact] void should_reject_it() => _result.IsSuccess.ShouldBeFalse();
    [Fact] void should_say_why() => Rejection().ShouldEqual(OnBehalfTokenRejection.Malformed);
}
#endif
