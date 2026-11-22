// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Playbill.given;

namespace Cratis.Showrunner.Playbill.Endpoints.for_PlaybillEndpoints.when_getting_a_release;

/// <summary>
/// A caller that does not ask for JSON gets the note as it was captured.
/// </summary>
public class as_markdown : an_endpoint_call
{
    async Task Because() => await Execute(PlaybillEndpoints.Get(_notes, "1.1.0", _http));

    [Fact] void should_succeed() => StatusCode.ShouldEqual(200);

    [Fact] void should_be_markdown() => _http.Response.ContentType.ShouldEqual("text/markdown; charset=utf-8");

    [Fact] void should_be_the_note() => Text.ShouldEqual("Faster startup.\n\n## Fixed\n\n- A crash is gone.");

    [Fact] void should_tell_caches_that_the_answer_depends_on_what_was_accepted() => _http.Response.Headers.Vary.ToString().ShouldContain("Accept");
}
#endif
