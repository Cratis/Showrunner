// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Gating.for_ModuleCommandFilter.given;
using Cratis.Showrunner.BoxOffice.given;

namespace Cratis.Showrunner.BoxOffice.Gating.for_ModuleCommandFilter.when_filtering;

public class and_box_office_is_not_added : a_module_command_filter
{
    protected override bool BoxOfficeIsAdded => false;

    async Task Because() => await Filter(typeof(AnOrdinaryChange));

    [Fact] void should_let_the_command_through() => _result.ShouldBeAuthorized();
}
#endif
