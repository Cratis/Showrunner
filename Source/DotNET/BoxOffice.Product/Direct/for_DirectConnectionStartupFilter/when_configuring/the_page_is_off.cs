// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnectionStartupFilter.when_configuring;

public class the_page_is_off : Specification
{
    (int Get, int Post, int Other) _statuses;

    async Task Because() =>
        _statuses = await Pipeline.Run(new BoxOfficeProductOptions { Direct = new() { Enabled = false, Administrators = ["1001"] } }, Substitute.For<IDirectEnrollment>());

    [Fact] void should_not_have_the_route_for_a_get() => _statuses.Get.ShouldEqual(StatusCodes.Status404NotFound);
    [Fact] void should_not_have_the_route_for_a_post() => _statuses.Post.ShouldEqual(StatusCodes.Status404NotFound);
}
#endif
