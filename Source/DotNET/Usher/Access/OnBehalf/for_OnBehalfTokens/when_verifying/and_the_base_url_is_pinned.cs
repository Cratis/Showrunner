// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access.OnBehalf.for_OnBehalfTokens.given;

namespace Cratis.Showrunner.Usher.Access.OnBehalf.for_OnBehalfTokens.when_verifying;

public class and_the_base_url_is_pinned : on_behalf_tokens
{
    bool _sameHostAccepted;
    OnBehalfTokenRejection _otherHost;

    async Task Because()
    {
        _options.BaseUrl = new("https://portal.internal/");
        await Verify(GrantCheckToken());
        _sameHostAccepted = _result.IsSuccess;
        await Verify(_authProxy.ForCall(OnBehalfPaths.GrantCheckPurpose, HttpMethod.Get, new Uri("https://attacker.example.com/.cratis/on-behalf/grants"), [], Session, Now));
        _otherHost = Rejection();
    }

    [Fact] void should_accept_a_token_bound_to_it() => _sameHostAccepted.ShouldBeTrue();
    [Fact] void should_reject_a_token_bound_to_another_host() => _otherHost.ShouldEqual(OnBehalfTokenRejection.WrongTarget);
}
#endif
