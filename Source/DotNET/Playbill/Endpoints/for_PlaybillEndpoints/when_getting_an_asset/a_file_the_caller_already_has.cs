// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Playbill.given;
using Microsoft.AspNetCore.Http;

namespace Cratis.Showrunner.Playbill.Endpoints.for_PlaybillEndpoints.when_getting_an_asset;

/// <summary>
/// A caller that repeats the request with the tag it was given is told the file has not changed.
/// </summary>
public class a_file_the_caller_already_has : an_endpoint_call
{
    async Task Because()
    {
        await Execute(PlaybillEndpoints.GetAsset(_notes, "1.1.0", "hero.png"));
        var tag = _http.Response.Headers.ETag.ToString();

        _body.SetLength(0);
        _http.Response.Headers.Clear();
        _http.Request.Headers.IfNoneMatch = tag;
        await Execute(PlaybillEndpoints.GetAsset(_notes, "1.1.0", "hero.png"));
    }

    [Fact] void should_say_it_has_not_changed() => StatusCode.ShouldEqual(StatusCodes.Status304NotModified);

    [Fact] void should_not_send_the_file_again() => _body.Length.ShouldEqual(0);
}
#endif
