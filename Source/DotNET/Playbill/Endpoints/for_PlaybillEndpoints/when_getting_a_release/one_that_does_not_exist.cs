// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Playbill.given;

namespace Cratis.Showrunner.Playbill.Endpoints.for_PlaybillEndpoints.when_getting_a_release;

/// <summary>
/// A release without a note is not found.
/// </summary>
public class one_that_does_not_exist : an_endpoint_call
{
    async Task Because() => await Execute(PlaybillEndpoints.Get(_notes, "9.9.9", _http));

    [Fact] void should_not_find_it() => StatusCode.ShouldEqual(404);
}
#endif
