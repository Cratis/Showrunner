// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Herald.Rendering;

namespace Cratis.Showrunner.Herald.for_EmailRenderer.when_rendering;

public class a_blank_required_value : given.a_templates_folder
{
    EmailCannotBeRendered _exception;

    void Because() => _exception = Assert.Throws<EmailCannotBeRendered>(() => _renderer.Render("invitation", null, Values(("invitation.link", "  "))));

    [Fact] void should_name_the_value_that_is_missing() => Assert.Contains("invitation.link", _exception.Message);
}
#endif
