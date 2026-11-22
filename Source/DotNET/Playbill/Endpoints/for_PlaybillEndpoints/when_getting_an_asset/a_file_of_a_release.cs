// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Playbill.given;

namespace Cratis.Showrunner.Playbill.Endpoints.for_PlaybillEndpoints.when_getting_an_asset;

/// <summary>
/// A file a note refers to is served with the type of its extension and a tag that lets a repeat request be answered without it.
/// </summary>
public class a_file_of_a_release : an_endpoint_call
{
    async Task Because() => await Execute(PlaybillEndpoints.GetAsset(_notes, "1.1.0", "hero.png"));

    [Fact] void should_succeed() => StatusCode.ShouldEqual(200);

    [Fact] void should_have_the_type_of_its_extension() => _http.Response.ContentType.ShouldEqual("image/png");

    [Fact] void should_be_the_file() => Text.ShouldEqual("not really a png");

    [Fact] void should_be_tagged() => _http.Response.Headers.ETag.ToString().ShouldNotBeEmpty();

    [Fact] void should_be_cacheable_by_the_signed_in_person_only() => _http.Response.Headers.CacheControl.ToString().StartsWith("private").ShouldBeTrue();
}
#endif
