// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access.OnBehalf.for_OnBehalfTokens.given;

namespace Cratis.Showrunner.Usher.Access.OnBehalf.for_OnBehalfTokens.when_verifying_a_forward;

public class from_authproxy : on_behalf_tokens
{
    const string PrincipalHeader = "eyJ1c2VySWQiOiJyZXBvcnRlciJ9";
    const string ActorHeader = "eyJ1c2VySWQiOiJzdXBwb3J0In0=";

    async Task Because() =>
        await Assign(_tokens.VerifyForward(_authProxy.ForForward(Session, PrincipalHeader, ActorHeader, Now), PrincipalHeader, ActorHeader));

    [Fact] void should_accept_it() => _result.IsSuccess.ShouldBeTrue();
}
#endif
