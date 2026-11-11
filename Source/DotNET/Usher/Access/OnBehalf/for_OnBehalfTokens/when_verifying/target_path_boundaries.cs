// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access.OnBehalf.for_OnBehalfTokens.given;

namespace Cratis.Showrunner.Usher.Access.OnBehalf.for_OnBehalfTokens.when_verifying;

public class target_path_boundaries : on_behalf_tokens
{
    bool _pathBaseAccepted;

    async Task Because()
    {
        var mounted = new Uri($"https://portal.internal/mounted{OnBehalfPaths.Grants}");
        await Verify(_authProxy.ForCall(OnBehalfPaths.GrantCheckPurpose, HttpMethod.Get, mounted, [], Session, Now), GrantCheck with { PathBase = "/mounted" });
        _pathBaseAccepted = _result.IsSuccess;
        _options.BaseUrl = new Uri("https://portal.internal");
        await Verify(_authProxy.ForCall(OnBehalfPaths.GrantCheckPurpose, HttpMethod.Get, new Uri("https://portal.internal/other"), [], Session, Now));
    }

    [Fact] void should_accept_the_unpinned_path_base() => _pathBaseAccepted.ShouldBeTrue();
    [Fact] void should_refuse_a_different_path_on_the_pinned_host() => Rejection().ShouldEqual(OnBehalfTokenRejection.WrongTarget);
}
#endif
