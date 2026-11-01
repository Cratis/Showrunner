// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Herald.Rendering;

namespace Cratis.Showrunner.Herald.for_TemplateStore.when_reading;

public class an_email_without_a_manifest : given.a_templates_folder
{
    EmailCannotBeRendered _exception;

    void Because() => _exception = Assert.Throws<EmailCannotBeRendered>(() => _renderer.Render("welcome", null, Values()));

    [Fact] void should_name_the_email() => Assert.Contains("'welcome'", _exception.Message);
}
#endif
