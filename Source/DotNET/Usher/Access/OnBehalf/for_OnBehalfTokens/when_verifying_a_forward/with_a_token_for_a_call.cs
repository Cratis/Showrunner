// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access.OnBehalf.for_OnBehalfTokens.given;

namespace Cratis.Showrunner.Usher.Access.OnBehalf.for_OnBehalfTokens.when_verifying_a_forward;

public class with_a_token_for_a_call : on_behalf_tokens
{
    async Task Because() => await Assign(_tokens.VerifyForward(GrantCheckToken(), "principal", "actor"));

    [Fact] void should_reject_it() => Rejection().ShouldEqual(OnBehalfTokenRejection.WrongPurpose);
}
#endif
