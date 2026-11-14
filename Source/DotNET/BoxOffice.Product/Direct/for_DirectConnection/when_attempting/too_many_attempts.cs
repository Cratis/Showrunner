// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnection.given;

namespace Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnection.when_attempting;

public class too_many_attempts : a_direct_connection
{
    DirectAttemptResult _result;

    async Task Because()
    {
        _options.Direct.MaximumAttempts = 2;
        _enrollment.Redeem(Arg.Any<DirectEnrollmentRequest>(), Arg.Any<CancellationToken>()).Returns(new DirectRedemptionResult(DirectRedemption.Refused));
        await _connection.Attempt(Code, null, CancellationToken.None);
        await _connection.Attempt(Code, null, CancellationToken.None);
        _result = await _connection.Attempt(Code, null, CancellationToken.None);
    }

    [Fact] void should_refuse_the_attempt_over_the_limit() => _result.ShouldEqual(DirectAttemptResult.TooManyAttempts);
    [Fact] void should_only_redeem_within_the_limit() => _enrollment.Received(2).Redeem(Arg.Any<DirectEnrollmentRequest>(), Arg.Any<CancellationToken>());
}
#endif
