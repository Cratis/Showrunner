// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Herald.Rendering;

namespace Cratis.Showrunner.Herald.for_EmailRenderer.when_rendering;

public class an_optional_value_left_out : given.a_templates_folder
{
    RenderedEmail _result;

    void Because() => _result = _renderer.Render("invitation", null, Values(("invitation.link", "https://ledger.example.com/join")));

    [Fact] void should_leave_out_the_conditional_section() => Assert.DoesNotContain("blockquote", _result.Html);
    [Fact] void should_leave_the_date_empty() => Assert.Contains("Expires </p>", _result.Html.Replace("\n", string.Empty));
}
#endif
