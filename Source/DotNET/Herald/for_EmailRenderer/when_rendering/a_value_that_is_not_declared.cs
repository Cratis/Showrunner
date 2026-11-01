// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Herald.Rendering;

namespace Cratis.Showrunner.Herald.for_EmailRenderer.when_rendering;

public class a_value_that_is_not_declared : given.a_templates_folder
{
    EmailCannotBeRendered _exception;

    void Because() => _exception = Assert.Throws<EmailCannotBeRendered>(() =>
        _renderer.Render("invitation", null, Values(("invitation.link", "https://x"), ("organisation.name", "Kestrel"))));

    [Fact] void should_name_the_value() => Assert.Contains("organisation.name", _exception.Message);
    [Fact] void should_not_repeat_what_the_value_held() => Assert.DoesNotContain("Kestrel", _exception.Message);
}
#endif
