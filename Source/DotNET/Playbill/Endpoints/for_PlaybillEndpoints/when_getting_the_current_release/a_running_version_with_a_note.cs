// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Text.Json;
using Cratis.Showrunner.Playbill.given;

namespace Cratis.Showrunner.Playbill.Endpoints.for_PlaybillEndpoints.when_getting_the_current_release;

/// <summary>
/// The current release names the version the application is running, its note and the newest note there is.
/// </summary>
public class a_running_version_with_a_note : an_endpoint_call
{
    JsonElement _current;

    async Task Because()
    {
        await Execute(PlaybillEndpoints.GetCurrent(_notes));
        _current = JsonDocument.Parse(Text).RootElement;
    }

    [Fact] void should_name_the_version() => _current.GetProperty("version").GetString().ShouldEqual("1.1.0");

    [Fact] void should_describe_its_release() => _current.GetProperty("release").GetProperty("summary").GetString().ShouldEqual("Faster startup.");

    [Fact] void should_name_the_newest_release() => _current.GetProperty("newest").GetString().ShouldEqual("1.1.0");
}
#endif
