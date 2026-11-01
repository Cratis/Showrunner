// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Herald.Rendering;

namespace Cratis.Showrunner.Herald.for_EmailRenderer.when_rendering;

public class the_norwegian_language : given.a_templates_folder
{
    RenderedEmail _result;

    void Because() => _result = _renderer.Render("invitation", "nb-NO", InvitationValues);

    [Fact] void should_use_the_template_of_the_language_without_its_region() => Assert.Equal("nb", _result.Language.Value);
    [Fact] void should_fill_the_subject() => Assert.Equal("Ada har invitert deg til Ledger", _result.Subject);
    [Fact] void should_format_the_date_in_norwegian() => Assert.Contains("Utløper 15. november", _result.Html);
}
#endif
