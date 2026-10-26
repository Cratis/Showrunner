// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access.OnBehalf.for_OnBehalfTokens.given;

namespace Cratis.Showrunner.Usher.Access.OnBehalf.for_OnBehalfTokens.when_verifying_a_forward;

public class with_identity_headers_it_was_not_issued_for : on_behalf_tokens
{
    const string PrincipalHeader = "eyJ1c2VySWQiOiJyZXBvcnRlciJ9";
    const string ActorHeader = "eyJ1c2VySWQiOiJzdXBwb3J0In0=";

    async Task Because() =>
        await Assign(_tokens.VerifyForward(_authProxy.ForForward(Session, PrincipalHeader, ActorHeader, Now), PrincipalHeader, "eyJ1c2VySWQiOiJzb21lYm9keSJ9"));

    [Fact] void should_reject_it() => Rejection().ShouldEqual(OnBehalfTokenRejection.WrongIdentity);
}
#endif
