// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Arc.Chronicle.Testing.Queries;
using Cratis.Arc.Identity;
using Cratis.Arc.Queries;
using Cratis.Arc.Testing.Queries;
using Cratis.Showrunner.GuestList.Authoring.given;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Showrunner.GuestList.Authoring.Decisions.for_ProductGuestList.when_observing;

/// <summary>
/// A caller the host's authoring policy does not admit sees nothing of who holds what.
/// </summary>
public class a_caller_that_is_not_an_operator : Specification
{
    QueryScenario<ProductGuestList> _scenario;
    QueryResult _result;

    void Establish()
    {
        _scenario = new();
        _scenario.Services.AddSingleton<ICurrentPrincipalAccessor>(new FixedPrincipal(Operators.Somebody));
        _scenario.Services.AddArcAuthorizationPolicy<AdmitOperators>(GuestListAuthoring.Policy);
    }

    void Destroy() => _scenario.Dispose();

    async Task Because() => _result = await _scenario.Perform(nameof(ProductGuestList.ForProduct), new QueryArguments { ["product"] = "ledger" });

    [Fact] void should_not_be_authorized() => _result.IsAuthorized.ShouldBeFalse();
}
#endif
