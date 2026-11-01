// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Herald.Rendering;

namespace Cratis.Showrunner.Herald.for_EmailRenderer.when_rendering;

public class an_svg_image : given.a_templates_folder
{
    EmailCannotBeRendered _exception;

    void Because()
    {
        Write("invitation.en.html", English.Replace("logo.png", "logo.svg", StringComparison.Ordinal));
        _exception = Assert.Throws<EmailCannotBeRendered>(() => _renderer.Render("invitation", null, InvitationValues));
    }

    [Fact] void should_say_it_is_an_svg() => Assert.Contains("SVG", _exception.Message);
    [Fact] void should_name_the_image() => Assert.Contains("logo.svg", _exception.Message);
}
#endif
