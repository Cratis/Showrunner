// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnection.given;

namespace Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnection.when_attempting;

public class a_code_that_is_refused : a_direct_connection
{
    DirectAttemptResult _result;

    async Task Because()
    {
        _enrollment.Redeem(Arg.Any<DirectEnrollmentRequest>(), Arg.Any<CancellationToken>()).Returns(new DirectRedemptionResult(DirectRedemption.Refused));
        _result = await _connection.Attempt(Code, null, CancellationToken.None);
    }

    [Fact] void should_refuse_the_code() => _result.ShouldEqual(DirectAttemptResult.CodeRefused);
    [Fact] void should_leave_the_page_open() => _record.State.Stage.ShouldEqual(DirectLinkStage.NotConfigured);
    [Fact] void should_not_try_to_connect() => _enrollment.DidNotReceive().Verify(Arg.Any<CancellationToken>());
}
#endif
