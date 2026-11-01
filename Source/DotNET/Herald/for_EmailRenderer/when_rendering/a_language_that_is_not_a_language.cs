// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Herald.Rendering;

namespace Cratis.Showrunner.Herald.for_EmailRenderer.when_rendering;

public class a_language_that_is_not_a_language : given.a_templates_folder
{
    [Fact] void should_refuse_a_path() => Assert.Throws<EmailCannotBeRendered>(() => _renderer.Render("invitation", "../../etc/passwd", InvitationValues));
    [Fact] void should_refuse_a_kind_that_is_a_path() => Assert.Throws<EmailCannotBeRendered>(() => _renderer.Render("../invitation", null, InvitationValues));
}
#endif
