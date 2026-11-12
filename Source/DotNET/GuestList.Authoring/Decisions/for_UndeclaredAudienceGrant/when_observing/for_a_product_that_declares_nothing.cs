// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Arc.Chronicle.Testing.Queries;
using Cratis.Arc.Identity;
using Cratis.Arc.Queries;
using Cratis.Arc.Testing.Queries;
using Cratis.Chronicle;
using Cratis.Showrunner.GuestList.Authoring.given;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.GuestList.Authoring.Decisions.for_UndeclaredAudienceGrant.when_observing;

/// <summary>
/// Without a declaration of the product's audiences to compare with, no grant can be called undeclared.
/// </summary>
public class for_a_product_that_declares_nothing : Specification
{
    QueryScenario<UndeclaredAudienceGrant> _scenario;
    QueryResult _result;

    void Establish()
    {
        _scenario = new();
        _scenario.Services.AddSingleton<ICurrentPrincipalAccessor>(new FixedPrincipal(Operators.Operator));
        _scenario.Services.AddArcAuthorizationPolicy<AdmitOperators>(GuestListAuthoring.Policy);
        _scenario.Services.AddSingleton(Options.Create(new GuestListAuthoringOptions()));
        _scenario.Given.ForEventSource(new EventSourceId("ledger")).Events(Guests.TenantGrant);
    }

    void Destroy() => _scenario.Dispose();

    async Task Because() => _result = await _scenario.Perform(nameof(UndeclaredAudienceGrant.ForProduct), new QueryArguments { ["product"] = "ledger" });

    [Fact] void should_succeed() => _result.IsSuccess.ShouldBeTrue();

    [Fact] void should_report_nothing() => ((IEnumerable<UndeclaredAudienceGrant>)_result.Data).ShouldBeEmpty();
}
#endif
