// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Herald.Rendering;

namespace Cratis.Showrunner.Herald.for_EmailRenderer.when_rendering;

public class an_image_that_is_not_in_the_folder : given.a_templates_folder
{
    EmailCannotBeRendered _exception;

    void Because()
    {
        Write("invitation.en.html", English.Replace("logo.png", "banner.png", StringComparison.Ordinal));
        _exception = Assert.Throws<EmailCannotBeRendered>(() => _renderer.Render("invitation", null, InvitationValues));
    }

    [Fact] void should_name_the_image() => Assert.Contains("banner.png", _exception.Message);
}
#endif
