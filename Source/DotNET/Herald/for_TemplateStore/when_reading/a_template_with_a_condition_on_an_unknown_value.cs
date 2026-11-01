// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Herald.Rendering;

namespace Cratis.Showrunner.Herald.for_TemplateStore.when_reading;

public class a_template_with_a_condition_on_an_unknown_value : given.a_templates_folder
{
    EmailCannotBeRendered _exception;

    void Because()
    {
        Write("invitation.en.html", "---\nsubject: Hello\n---\n<p>{{#if organisation.name}}x{{/if}}</p>");
        _exception = Assert.Throws<EmailCannotBeRendered>(() => _renderer.Render("invitation", null, InvitationValues));
    }

    [Fact] void should_refuse_the_template() => Assert.Contains("organisation.name", _exception.Message);
}
#endif
