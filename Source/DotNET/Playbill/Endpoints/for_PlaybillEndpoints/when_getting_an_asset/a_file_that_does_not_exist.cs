// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Playbill.given;

namespace Cratis.Showrunner.Playbill.Endpoints.for_PlaybillEndpoints.when_getting_an_asset;

/// <summary>
/// Only a file of a release is served: a path that climbs out of the folder, or names the note, finds nothing.
/// </summary>
public class a_file_that_does_not_exist : an_endpoint_call
{
    [Fact] async Task should_not_find_a_file_the_release_does_not_have()
    {
        await Execute(PlaybillEndpoints.GetAsset(_notes, "1.1.0", "missing.png"));
        StatusCode.ShouldEqual(404);
    }

    [Fact] async Task should_not_serve_a_path_that_climbs_out_of_the_folder()
    {
        await Execute(PlaybillEndpoints.GetAsset(_notes, "1.1.0", "../1.0.0/1.0.0.md"));
        StatusCode.ShouldEqual(404);
    }

    [Fact] async Task should_not_serve_a_file_of_a_release_that_does_not_exist()
    {
        await Execute(PlaybillEndpoints.GetAsset(_notes, "9.9.9", "hero.png"));
        StatusCode.ShouldEqual(404);
    }
}
#endif
