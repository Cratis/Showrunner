// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access.OnBehalf.for_OnBehalfTokens.given;

namespace Cratis.Showrunner.Usher.Access.OnBehalf.for_OnBehalfTokens.when_verifying_a_forward;

public class without_an_assertion : on_behalf_tokens
{
    async Task Because() => await Assign(_tokens.VerifyForward(string.Empty, "principal", "actor"));

    [Fact] void should_reject_it() => Rejection().ShouldEqual(OnBehalfTokenRejection.Missing);
}
#endif
