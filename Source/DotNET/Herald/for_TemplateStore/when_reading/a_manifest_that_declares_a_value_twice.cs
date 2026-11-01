// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Herald.Rendering;

namespace Cratis.Showrunner.Herald.for_TemplateStore.when_reading;

public class a_manifest_that_declares_a_value_twice : given.a_templates_folder
{
    [Fact] void should_refuse_the_manifest() => Assert.Throws<EmailCannotBeRendered>(() => EmailManifest.Parse("x", """{ "values": ["a.b", "a.b"] }"""));
    [Fact] void should_refuse_a_value_that_is_also_a_group() => Assert.Throws<EmailCannotBeRendered>(() => EmailManifest.Parse("x", """{ "values": ["a", "a.b"] }"""));
    [Fact] void should_refuse_the_look_values() => Assert.Throws<EmailCannotBeRendered>(() => EmailManifest.Parse("x", """{ "values": ["look.footer"] }"""));
}
#endif
