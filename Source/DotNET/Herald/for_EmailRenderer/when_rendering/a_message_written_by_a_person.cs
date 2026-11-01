// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Herald.Rendering;

namespace Cratis.Showrunner.Herald.for_EmailRenderer.when_rendering;

public class a_message_written_by_a_person : given.a_templates_folder
{
    RenderedEmail _result;

    void Because() => _result = _renderer.Render(
        "invitation",
        null,
        Values(
            ("invitation.link", "https://ledger.example.com/join"),
            ("inviter.name", "<b>Ada</b>"),
            ("inviter.message", "<script>alert('x')</script><a href=\"https://evil.example\">click</a>")));

    [Fact] void should_not_let_the_message_inject_a_script() => Assert.DoesNotContain("<script", _result.Html, StringComparison.OrdinalIgnoreCase);
    [Fact] void should_not_let_the_message_inject_a_link() => Assert.DoesNotContain("<a href=\"https://evil.example\"", _result.Html);
    [Fact] void should_show_the_message_as_text() => Assert.Contains("&lt;script&gt;", _result.Html);
    [Fact] void should_show_the_message_as_text_in_plain_text() => Assert.Contains("<script>alert('x')</script>", _result.Text);
    [Fact] void should_keep_the_subject_as_plain_text() => Assert.Equal("<b>Ada</b> invited you to", _result.Subject);
}
#endif
