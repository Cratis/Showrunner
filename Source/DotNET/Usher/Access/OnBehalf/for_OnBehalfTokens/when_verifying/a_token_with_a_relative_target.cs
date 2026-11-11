// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access.OnBehalf.for_OnBehalfTokens.given;

namespace Cratis.Showrunner.Usher.Access.OnBehalf.for_OnBehalfTokens.when_verifying;

public class a_token_with_a_relative_target : on_behalf_tokens
{
    Task Because() => Verify(
        _authProxy.ForForward(Session, "principal", "actor", Now),
        GrantCheck with { Purpose = "on-behalf-forward" });

    [Fact] void should_say_why() => Rejection().ShouldEqual(OnBehalfTokenRejection.WrongTarget);
}
#endif
