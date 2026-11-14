// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnectionCalls.given;

namespace Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnectionCalls.when_receiving;

public class too_many_attempts_on_the_page : a_direct_page
{
    Answer _answer;

    async Task Because()
    {
        _options.Direct.MaximumAttempts = 1;
        _enrollment.Redeem(Arg.Any<DirectEnrollmentRequest>(), Arg.Any<CancellationToken>()).Returns(new DirectRedemptionResult(DirectRedemption.Refused));
        await Submit(Code);
        _answer = await Submit(Code);
    }

    [Fact] void should_answer_too_many_requests() => _answer.Status.ShouldEqual(StatusCodes.Status429TooManyRequests);
}
#endif
