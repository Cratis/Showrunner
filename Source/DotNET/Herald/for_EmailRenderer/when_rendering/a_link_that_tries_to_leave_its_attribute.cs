// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Herald.Rendering;

namespace Cratis.Showrunner.Herald.for_EmailRenderer.when_rendering;

public class a_link_that_tries_to_leave_its_attribute : given.a_templates_folder
{
    RenderedEmail _result;

    void Because() => _result = _renderer.Render(
        "invitation",
        null,
        Values(("invitation.link", "https://ledger.example.com/\" onmouseover=\"alert(1)")));

    [Fact] void should_not_add_an_attribute() => Assert.DoesNotContain("onmouseover=\"", _result.Html);
    [Fact] void should_keep_the_quote_inside_the_value() => Assert.Contains("&quot; onmouseover=&quot;alert(1)", _result.Html);
}
#endif
