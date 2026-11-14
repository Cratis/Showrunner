// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnectionCalls.given;

namespace Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnectionCalls.when_receiving;

public class a_refused_code_posted : a_direct_page
{
    Answer _answer;

    async Task Because()
    {
        _enrollment.Redeem(Arg.Any<DirectEnrollmentRequest>(), Arg.Any<CancellationToken>()).Returns(new DirectRedemptionResult(DirectRedemption.Refused));
        _answer = await Submit(Code);
    }

    [Fact] void should_say_so_in_a_constant_message() => _answer.Html.ShouldContain("The enrollment code was not accepted.");
    [Fact] void should_not_echo_the_code() => _answer.Html.ShouldNotContain(Code);
    [Fact] void should_keep_asking_for_a_code() => _answer.Html.ShouldContain("name=\"code\"");
}
#endif
