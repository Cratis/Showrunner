// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnectionCalls.given;

namespace Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnectionCalls.when_receiving;

public class a_failed_connection_and_then_a_retry_on_the_page : a_direct_page
{
    Answer _failed;
    Answer _retry;

    async Task Because()
    {
        Redeems();
        Connects(DirectLinkHealth.Unreachable);
        _failed = await Submit(Code);
        Connects(DirectLinkHealth.Healthy);
        _retry = await Submit(string.Empty);
    }

    [Fact] void should_show_why_it_failed() => _failed.Html.ShouldContain("connecting to Chronicle failed");
    [Fact] void should_not_ask_for_the_code_again() => _failed.Html.ShouldNotContain("name=\"code\"");
    [Fact] void should_connect_on_the_retry() => _retry.Html.ShouldContain("is connected to Chronicle");
    [Fact] void should_redeem_only_one_code() => _enrollment.Received(1).Redeem(Arg.Any<DirectEnrollmentRequest>(), Arg.Any<CancellationToken>());
}
#endif
