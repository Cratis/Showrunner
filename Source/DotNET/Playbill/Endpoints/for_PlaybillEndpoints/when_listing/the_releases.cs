// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Text.Json;
using Cratis.Showrunner.Playbill.given;

namespace Cratis.Showrunner.Playbill.Endpoints.for_PlaybillEndpoints.when_listing;

/// <summary>
/// The list gives every release, the newest first, with what is needed to show a list without fetching each note.
/// </summary>
public class the_releases : an_endpoint_call
{
    JsonElement _list;

    async Task Because()
    {
        await Execute(PlaybillEndpoints.List(_notes, null));
        _list = JsonDocument.Parse(Text).RootElement;
    }

    [Fact] void should_succeed() => StatusCode.ShouldEqual(200);

    [Fact] void should_name_the_running_version() => _list.GetProperty("current").GetString().ShouldEqual("1.1.0");

    [Fact] void should_list_the_newest_first() => _list.GetProperty("releases").EnumerateArray().Select(_ => _.GetProperty("version").GetString()).ShouldEqual(["1.1.0", "1.0.0"]);

    [Fact] void should_say_what_a_release_is_about() => _list.GetProperty("releases")[0].GetProperty("summary").GetString().ShouldEqual("Faster startup.");

    [Fact] void should_name_the_sections_of_a_release() => _list.GetProperty("releases")[0].GetProperty("sections").EnumerateArray().Select(_ => _.GetString()).ShouldContainOnly("Fixed");

    [Fact] void should_say_when_and_from_which_pull_request() => (_list.GetProperty("releases")[0].GetProperty("releasedAt").GetDateTimeOffset(), _list.GetProperty("releases")[0].GetProperty("pull").GetString())
        .ShouldEqual((new DateTimeOffset(2026, 3, 2, 10, 0, 0, TimeSpan.Zero), "12"));

    [Fact] void should_not_carry_the_notes_themselves() => _list.GetProperty("releases")[0].TryGetProperty("markdown", out _).ShouldBeFalse();
}
#endif
