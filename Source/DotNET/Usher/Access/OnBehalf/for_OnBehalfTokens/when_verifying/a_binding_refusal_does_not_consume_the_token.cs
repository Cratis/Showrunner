// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access.OnBehalf.for_OnBehalfTokens.given;

namespace Cratis.Showrunner.Usher.Access.OnBehalf.for_OnBehalfTokens.when_verifying;

public class a_binding_refusal_does_not_consume_the_token : on_behalf_tokens
{
    OnBehalfTokenRejection _badBody;
    bool _accepted;

    async Task Because()
    {
        var token = GrantCheckToken();
        await Verify(token, GrantCheck with { Body = new byte[] { 1 } });
        _badBody = Rejection();
        await Verify(token);
        _accepted = _result.IsSuccess;
        await Verify(token, GrantCheck with { Method = "POST" });
    }

    [Fact] void should_refuse_the_wrong_body() => _badBody.ShouldEqual(OnBehalfTokenRejection.WrongBody);
    [Fact] void should_accept_the_same_token_on_its_bound_call() => _accepted.ShouldBeTrue();
    [Fact] void should_check_binding_before_replay() => Rejection().ShouldEqual(OnBehalfTokenRejection.WrongMethod);
}
#endif
