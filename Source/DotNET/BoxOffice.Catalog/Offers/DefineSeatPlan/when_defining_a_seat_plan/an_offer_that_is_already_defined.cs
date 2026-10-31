// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Catalog.given;

namespace Cratis.Showrunner.BoxOffice.Catalog.Offers.DefineSeatPlan.when_defining_a_seat_plan;

public class an_offer_that_is_already_defined : an_offer_command<DefineSeatPlan>
{
    void Establish() => Given(OfferEvents.Team);

    async Task Because() => _result = await _scenario.Execute(new DefineSeatPlan(_offerId, "Team", "For small teams.", 10, false, false));

    [Fact] void should_not_succeed() => _result.ShouldNotBeSuccessful();
    [Fact] void should_have_validation_errors() => _result.ShouldHaveValidationErrors();
    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
