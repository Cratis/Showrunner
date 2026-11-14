// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnectionCalls.given;

namespace Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnectionCalls.when_receiving;

public class a_valid_code_posted : a_direct_page
{
    Answer _answer;

    async Task Because()
    {
        Redeems();
        Connects(DirectLinkHealth.Healthy);
        _answer = await Submit(Code);
    }

    [Fact] void should_answer_that_it_is_connected() => _answer.Html.ShouldContain("is connected to Chronicle");
    [Fact] void should_close_the_link() => _record.State.Stage.ShouldEqual(DirectLinkStage.Closed);
    [Fact] void should_not_echo_the_code() => _answer.Html.ShouldNotContain(Code);
    [Fact] void should_hide_the_form() => _answer.Html.ShouldContain("hidden");
}
#endif
