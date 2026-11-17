// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnectionCalls.given;

namespace Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnectionCalls.when_receiving;

public class the_page_for_an_administrator : a_direct_page
{
    Answer _answer;

    async Task Because() => _answer = await Open();

    [Fact] void should_show_the_page() => _answer.Status.ShouldEqual(StatusCodes.Status200OK);
    [Fact] void should_ask_for_the_code() => _answer.Html.ShouldContain("name=\"code\"");
    [Fact] void should_not_ask_for_an_endpoint_when_no_host_is_pinned() => _answer.Html.ShouldNotContain("name=\"endpoint\"");
    [Fact] void should_carry_a_forgery_token_for_the_person() => _forgery.IsValid(_answer.Token, Administrator).ShouldBeTrue();
    [Fact] void should_set_no_cookie() => _answer.Headers.SetCookie.Count.ShouldEqual(0);
    [Fact] void should_say_it_is_connecting_when_the_form_is_sent() => _answer.Html.ShouldContain("Connecting to Chronicle");
    [Fact] void should_allow_only_its_own_script() => _answer.Headers.ContentSecurityPolicy.ToString().ShouldContain("script-src 'sha256-");
    [Fact] void should_not_be_cached() => _answer.Headers.CacheControl.ToString().ShouldEqual("no-store");
    [Fact] void should_make_no_external_requests() => _answer.Html.ShouldNotContain("http");
    [Fact] void should_forbid_framing() => _answer.Headers.XFrameOptions.ToString().ShouldEqual("DENY");
}
#endif
