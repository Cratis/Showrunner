// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Text.Json;
using Cratis.Showrunner.Playbill.given;

namespace Cratis.Showrunner.Playbill.Endpoints.for_PlaybillEndpoints.when_getting_a_release;

/// <summary>
/// A caller that asks for JSON gets the note split into its sections.
/// </summary>
public class as_json : an_endpoint_call
{
    JsonElement _release;

    async Task Because()
    {
        _http.Request.Headers.Accept = "application/json";
        await Execute(PlaybillEndpoints.Get(_notes, "1.1.0", _http));
        _release = JsonDocument.Parse(Text).RootElement;
    }

    [Fact] void should_succeed() => StatusCode.ShouldEqual(200);

    [Fact] void should_be_json() => _http.Response.ContentType.ShouldContain("json");

    [Fact] void should_name_the_version() => _release.GetProperty("version").GetString().ShouldEqual("1.1.0");

    [Fact] void should_give_what_comes_before_the_sections() => _release.GetProperty("lead").GetString().ShouldEqual("Faster startup.");

    [Fact] void should_give_each_section() => _release.GetProperty("sections").EnumerateArray().Select(_ => _.GetProperty("name").GetString()).ShouldContainOnly("Fixed");

    [Fact] void should_give_what_is_in_a_section() => _release.GetProperty("sections")[0].GetProperty("markdown").GetString().ShouldEqual("- A crash is gone.");

    [Fact] void should_list_the_assets() => _release.GetProperty("assets").EnumerateArray().Select(_ => _.GetString()).ShouldContainOnly("hero.png");
}
#endif
