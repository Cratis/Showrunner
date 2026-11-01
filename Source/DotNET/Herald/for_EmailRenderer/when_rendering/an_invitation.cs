// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Herald.Rendering;

namespace Cratis.Showrunner.Herald.for_EmailRenderer.when_rendering;

public class an_invitation : given.a_templates_folder
{
    RenderedEmail _result;

    void Because() => _result = _renderer.Render("invitation", null, InvitationValues);

    [Fact] void should_fill_the_subject() => Assert.Equal("Ada invited you to Ledger", _result.Subject);
    [Fact] void should_use_the_default_language() => Assert.Equal("en", _result.Language.Value);
    [Fact] void should_leave_no_placeholder() => Assert.DoesNotContain("{{", _result.Html);
    [Fact] void should_encode_the_link_for_html() => Assert.Contains("href=\"https://ledger.example.com/join?token=abc&amp;x=1\"", _result.Html);
    [Fact] void should_move_the_styles_inline() => Assert.Contains("background-color", _result.Html.Substring(_result.Html.IndexOf("<a ", StringComparison.Ordinal), 120));
    [Fact] void should_use_the_brand_color() => Assert.Contains("#0b5fff", _result.Html);
    [Fact] void should_add_the_footer() => Assert.Contains("Ledger AS, Oslo", _result.Html);
    [Fact] void should_add_the_preview_text_first_in_the_body() => Assert.Matches(@"<body[^>]*><div style=""display:none[^>]*>Accept the invitation to Ledger</div>", _result.Html);
    [Fact] void should_format_the_date_in_the_language() => Assert.Contains("Expires 15 November", _result.Html);
    [Fact] void should_embed_the_image_it_refers_to() => Assert.Equal(["logo.png"], _result.Images.Select(image => image.Name));
    [Fact] void should_give_the_image_its_media_type() => Assert.Equal("image/png", _result.Images[0].MediaType);
    [Fact] void should_generate_plain_text_with_the_link() => Assert.Contains("Accept (https://ledger.example.com/join?token=abc&x=1)", _result.Text);
    [Fact] void should_generate_plain_text_without_markup() => Assert.DoesNotContain("<", _result.Text);
    [Fact] void should_not_show_the_hidden_blockquote() => Assert.DoesNotContain("blockquote", _result.Html);
}
#endif
