// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Choreography.Availability.Context;
using Cratis.Showrunner.BoxOffice.Product.Availability.given;

namespace Cratis.Showrunner.BoxOffice.Product.Availability.for_EntitlementsHostContextValues;

public class when_asked_for_another_value : a_tenant_with_entitlements
{
    ContextValueOutcome _outcome;

    async Task Because() => _outcome = await _host.Get("vessel", CancellationToken.None);

    [Fact] void should_not_supply_it() => _outcome.IsPresent.ShouldBeFalse();
    [Fact] void should_not_read_the_entitlements() => _entitlementsRequested.ShouldEqual(0);
}
#endif
