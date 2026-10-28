// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Cryptography;
using Cratis.Showrunner.Usher.Access.OnBehalf.for_OnBehalfTokens.given;

namespace Cratis.Showrunner.Usher.Access.OnBehalf.for_OnBehalfTokens.when_verifying;

public class a_token_signed_with_another_key : on_behalf_tokens
{
    async Task Because()
    {
        using var key = RSA.Create(2048);
        await Verify(_authProxy.ForCall(OnBehalfPaths.GrantCheckPurpose, HttpMethod.Get, Target, [], Session, Now, key: key));
    }

    [Fact] void should_reject_it() => _result.IsSuccess.ShouldBeFalse();
    [Fact] void should_say_why() => Rejection().ShouldEqual(OnBehalfTokenRejection.InvalidSignature);
}
#endif
