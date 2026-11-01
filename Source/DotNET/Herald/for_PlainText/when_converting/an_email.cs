// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Herald.Rendering;

namespace Cratis.Showrunner.Herald.for_PlainText.when_converting;

public class an_email : Specification
{
    string _result;

    void Because() => _result = PlainText.From("""
        <html><head><title>Hidden</title><style>p { color: red; }</style></head>
        <body><h1>Welcome</h1><p>Hello <b>Ada</b> &amp; friends.</p>
        <ul><li>One</li><li>Two</li></ul>
        <img src="cid:logo.png" alt="Ledger logo">
        <p><a href="https://example.com/a?b=1">Open</a></p><p><a href="https://example.com">https://example.com</a></p>
        <p><a href="mailto:help@example.com">help@example.com</a><br>Bye</p></body></html>
        """);

    [Fact] void should_leave_out_the_head() => Assert.DoesNotContain("Hidden", _result);
    [Fact] void should_leave_out_the_styles() => Assert.DoesNotContain("color", _result);
    [Fact] void should_decode_entities() => Assert.Contains("Hello Ada & friends.", _result);
    [Fact] void should_put_list_items_on_lines() => Assert.Contains("- One\n- Two", _result);
    [Fact] void should_use_the_alternative_text_of_an_image() => Assert.Contains("Ledger logo", _result);
    [Fact] void should_show_where_a_link_goes() => Assert.Contains("Open (https://example.com/a?b=1)", _result);
    [Fact] void should_not_repeat_an_address_that_is_the_text() => Assert.Contains("\nhttps://example.com", _result);
    [Fact] void should_not_repeat_a_mail_address() => Assert.Contains("help@example.com\nBye", _result);
    [Fact] void should_have_no_markup() => Assert.DoesNotContain("<", _result);
}
#endif
