// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnectionCalls.given;

namespace Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnectionCalls.when_receiving;

public class a_hostile_endpoint_posted : a_direct_page
{
    Answer _answer;

    async Task Because()
    {
        Redeems();
        _answer = await Submit(Code, "https://attacker.example.net");
    }

    [Fact] void should_say_the_endpoint_is_not_allowed() => _answer.Html.ShouldContain("The endpoint is not allowed.");
    [Fact] void should_not_echo_the_endpoint() => _answer.Html.ShouldNotContain("attacker");
    [Fact] void should_not_redeem_a_code() => _enrollment.DidNotReceive().Redeem(Arg.Any<DirectEnrollmentRequest>(), Arg.Any<CancellationToken>());
}
#endif
