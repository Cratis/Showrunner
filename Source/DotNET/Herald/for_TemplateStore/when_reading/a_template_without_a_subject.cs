// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Herald.Rendering;

namespace Cratis.Showrunner.Herald.for_TemplateStore.when_reading;

public class a_template_without_a_subject : given.a_templates_folder
{
    EmailCannotBeRendered _exception;

    void Because()
    {
        Write("invitation.en.html", "---\npreview: Hello\n---\n<p>x</p>");
        _exception = Assert.Throws<EmailCannotBeRendered>(() => _renderer.Render("invitation", null, InvitationValues));
    }

    [Fact] void should_say_the_subject_is_missing() => Assert.Contains("no subject", _exception.Message);
}
#endif
