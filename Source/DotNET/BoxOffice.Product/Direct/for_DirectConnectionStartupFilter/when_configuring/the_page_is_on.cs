// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnectionStartupFilter.when_configuring;

public class the_page_is_on : Specification
{
    (int Get, int Post, int Other) _statuses;

    async Task Because() =>
        _statuses = await Pipeline.Run(new BoxOfficeProductOptions { Direct = new() { Enabled = true } }, Substitute.For<IDirectEnrollment>());

    [Fact] void should_answer_the_path_through_the_page_which_wants_a_person() => _statuses.Get.ShouldEqual(StatusCodes.Status401Unauthorized);
    [Fact] void should_leave_other_paths_alone() => _statuses.Other.ShouldEqual(StatusCodes.Status404NotFound);
}
#endif
