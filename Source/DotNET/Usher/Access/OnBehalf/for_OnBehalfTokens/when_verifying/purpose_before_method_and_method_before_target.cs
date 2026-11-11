// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access.OnBehalf.for_OnBehalfTokens.given;

namespace Cratis.Showrunner.Usher.Access.OnBehalf.for_OnBehalfTokens.when_verifying;

public class purpose_before_method_and_method_before_target : on_behalf_tokens
{
    OnBehalfTokenRejection _wrongPurpose;

    async Task Because()
    {
        await Verify(_authProxy.ForCall("another-purpose", HttpMethod.Post, new Uri("https://example.invalid/other"), [], Session with { SessionId = "" }, Now));
        _wrongPurpose = Rejection();
        await Verify(_authProxy.ForCall(OnBehalfPaths.GrantCheckPurpose, HttpMethod.Post, new Uri("https://example.invalid/other"), [], Session with { SessionId = "" }, Now));
    }

    [Fact] void should_refuse_purpose_before_binding_and_claims() => _wrongPurpose.ShouldEqual(OnBehalfTokenRejection.WrongPurpose);
    [Fact] void should_refuse_method_before_target_and_claims() => Rejection().ShouldEqual(OnBehalfTokenRejection.WrongMethod);
}
#endif
