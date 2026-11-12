// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Arc.Chronicle.Testing.Queries;
using Cratis.Arc.Identity;
using Cratis.Arc.Queries;
using Cratis.Arc.Testing.Queries;
using Cratis.Chronicle;
using Cratis.Showrunner.GuestList.Authoring.given;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Showrunner.GuestList.Authoring.Decisions.for_ProductGuestList.when_observing;

/// <summary>
/// The operator's screen shows the audiences granted for a product, and nobody else's.
/// </summary>
public class for_a_product : Specification
{
    QueryScenario<ProductGuestList> _scenario;
    QueryResult _result;

    void Establish()
    {
        _scenario = new();
        _scenario.Services.AddSingleton<ICurrentPrincipalAccessor>(new FixedPrincipal(Operators.Operator));
        _scenario.Services.AddArcAuthorizationPolicy<AdmitOperators>(GuestListAuthoring.Policy);
        _scenario.Given.ForEventSource(new EventSourceId("ledger")).Events(Guests.TenantGrant, Guests.UserGrant);
        _scenario.Given.ForEventSource(new EventSourceId("studio")).Events(Guests.SystemGrant);
    }

    void Destroy() => _scenario.Dispose();

    async Task Because() => _result = await _scenario.Perform(nameof(ProductGuestList.ForProduct), new QueryArguments { ["product"] = "ledger" });

    [Fact] void should_succeed() => _result.IsSuccess.ShouldBeTrue();

    [Fact] void should_return_the_guest_list_of_the_product_only() => ((IEnumerable<ProductGuestList>)_result.Data).Single().Product.ShouldEqual(Guests.Product);

    [Fact] void should_return_what_is_held() => ((IEnumerable<ProductGuestList>)_result.Data).Single().Grants.Count().ShouldEqual(2);
}
#endif
