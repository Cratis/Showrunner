// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnection.given;

namespace Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnection.when_attempting;

public class a_link_that_is_closed : a_direct_connection
{
    DirectAttemptResult _result;

    async Task Because()
    {
        Redeems();
        Connects(DirectLinkHealth.Healthy);
        await _connection.Attempt(Code, null, CancellationToken.None);
        _enrollment.ClearReceivedCalls();
        _result = await _connection.Attempt(Code, null, CancellationToken.None);
    }

    [Fact] void should_answer_that_it_is_closed() => _result.ShouldEqual(DirectAttemptResult.Closed);
    [Fact] void should_not_redeem_again() => _enrollment.DidNotReceive().Redeem(Arg.Any<DirectEnrollmentRequest>(), Arg.Any<CancellationToken>());
    [Fact] void should_not_ask_the_link_when_re_enrollment_is_not_allowed() => _enrollment.DidNotReceive().Verify(Arg.Any<CancellationToken>());
}
#endif
