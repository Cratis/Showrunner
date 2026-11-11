// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access.OnBehalf.for_OnBehalfTokens.given;

namespace Cratis.Showrunner.Usher.Access.OnBehalf.for_OnBehalfTokens.when_verifying;

public class authorization_header_boundaries : on_behalf_tokens
{
    OnBehalfTokenRejection _basic;
    OnBehalfTokenRejection _bare;

    async Task Because()
    {
        await Assign(_tokens.Verify("Basic x", GrantCheck));
        _basic = Rejection();
        await Assign(_tokens.Verify("Bearer ", GrantCheck));
        _bare = Rejection();
        await Assign(_tokens.Verify($"bearer  {GrantCheckToken()} ", GrantCheck));
    }

    [Fact] void should_refuse_another_scheme() => _basic.ShouldEqual(OnBehalfTokenRejection.Missing);
    [Fact] void should_refuse_a_bare_bearer_prefix() => _bare.ShouldEqual(OnBehalfTokenRejection.Missing);
    [Fact] void should_accept_lowercase_bearer_and_trim_the_token() => _result.IsSuccess.ShouldBeTrue();
}
#endif
