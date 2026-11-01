// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Herald.Rendering;

namespace Cratis.Showrunner.Herald.for_EmailRenderer.when_rendering;

public class a_language_without_a_template : given.a_templates_folder
{
    RenderedEmail _result;

    void Because() => _result = _renderer.Render("invitation", "fr", InvitationValues);

    [Fact] void should_fall_back_to_the_default_language() => Assert.Equal("en", _result.Language.Value);
}
#endif
