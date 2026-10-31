// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Catalog.given;

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers.DefineSeatPlan.when_defining_a_seat_plan;

public class a_plan_without_a_name : an_offer_command<DefineSeatPlan>
{
    async Task Because() => _result = await _scenario.Execute(new DefineSeatPlan(_offerId, "", "For small teams.", 10, false, false));

    [Fact] void should_not_succeed() => _result.ShouldNotBeSuccessful();
    [Fact] void should_have_validation_errors() => _result.ShouldHaveValidationErrors();
    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
