// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Text.Json;
using Cratis.Showrunner.Playbill.given;

namespace Cratis.Showrunner.Playbill.Endpoints.for_PlaybillEndpoints.when_listing;

/// <summary>
/// Asking for the releases since a version gives only the newer ones.
/// </summary>
public class the_releases_since_a_version : an_endpoint_call
{
    async Task Because() => await Execute(PlaybillEndpoints.List(_notes, "1.0.0"));

    [Fact] void should_succeed() => StatusCode.ShouldEqual(200);

    [Fact] void should_list_only_the_newer_releases() => JsonDocument.Parse(Text).RootElement.GetProperty("releases").EnumerateArray().Select(_ => _.GetProperty("version").GetString()).ShouldContainOnly("1.1.0");
}
#endif
