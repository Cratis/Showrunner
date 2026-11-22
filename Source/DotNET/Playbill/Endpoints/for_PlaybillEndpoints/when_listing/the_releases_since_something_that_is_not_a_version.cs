// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Playbill.given;

namespace Cratis.Showrunner.Playbill.Endpoints.for_PlaybillEndpoints.when_listing;

/// <summary>
/// Something that is not a version is a bad request, not an empty list.
/// </summary>
public class the_releases_since_something_that_is_not_a_version : an_endpoint_call
{
    async Task Because() => await Execute(PlaybillEndpoints.List(_notes, "yesterday"));

    [Fact] void should_refuse_the_request() => StatusCode.ShouldEqual(400);
}
#endif
