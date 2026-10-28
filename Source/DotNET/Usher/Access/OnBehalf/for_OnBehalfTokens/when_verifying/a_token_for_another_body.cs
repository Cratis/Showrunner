// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access.OnBehalf.for_OnBehalfTokens.given;

namespace Cratis.Showrunner.Usher.Access.OnBehalf.for_OnBehalfTokens.when_verifying;

public class a_token_for_another_body : on_behalf_tokens
{
    async Task Because()
    {
        var call = new OnBehalfCall(OnBehalfPaths.SessionEndedPurpose, "POST", string.Empty, OnBehalfPaths.SessionEnded, "{\"reason\":\"ended-by-actor\"}"u8.ToArray());
        var token = _authProxy.ForCall(OnBehalfPaths.SessionEndedPurpose, HttpMethod.Post, new Uri("https://portal.internal/.cratis/on-behalf/sessions/ended"), "{\"reason\":\"grant-not-live\"}"u8.ToArray(), Session, Now);
        await Verify(token, call);
    }

    [Fact] void should_reject_it() => _result.IsSuccess.ShouldBeFalse();
    [Fact] void should_say_why() => Rejection().ShouldEqual(OnBehalfTokenRejection.WrongBody);
}
#endif
