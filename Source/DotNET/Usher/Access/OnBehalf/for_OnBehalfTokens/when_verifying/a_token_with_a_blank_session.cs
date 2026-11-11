// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Access.OnBehalf.for_OnBehalfTokens.given;

namespace Cratis.Showrunner.Usher.Access.OnBehalf.for_OnBehalfTokens.when_verifying;

public class a_token_with_a_blank_session : on_behalf_tokens
{
    Task Because() => Verify(_authProxy.ForCall(OnBehalfPaths.GrantCheckPurpose, HttpMethod.Get, Target, [], Session with { SessionId = " " }, Now));

    [Fact] void should_accept_the_nonempty_session() => _result.IsSuccess.ShouldBeTrue();
}
#endif
