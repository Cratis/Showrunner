// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Choreography.Availability;
using Cratis.Choreography.Availability.Context;
using Cratis.Choreography.Availability.Context.Providers;
using Cratis.Showrunner.BoxOffice.Contracts;
using Cratis.Showrunner.BoxOffice.Contracts.Entitlements;
using Cratis.Showrunner.BoxOffice.Entitlements;
using Cratis.Showrunner.BoxOffice.Offering.Declaration;

namespace Cratis.Showrunner.BoxOffice.Product.Availability.given;

/// <summary>
/// A tenant of a product that includes a core, sells analytics and only previews reports, and an engine that lets a gate
/// read what Box Office entitles the tenant to.
/// </summary>
public class a_tenant_with_entitlements : Specification
{
    protected IEntitlements _entitlements;
    protected EntitlementsHostContextValues _host;
    protected int _entitlementsRequested;

    protected ContextValueResolver Request() => new ContextValueCatalog()
        .Declare(new ContextValueDeclaration(EntitlementsHostContextValues.Name, new HostContextValueProvider(_host, EntitlementsHostContextValues.Name), ContextScope.Application))
        .CreateResolver(ContextScope.Application, null, null);

    protected async Task<AvailabilityResult> Evaluate(string path, ComparisonOperator op = ComparisonOperator.IsTrue, params object[] operands) =>
        await new AvailabilityEvaluator(new AvailabilityCatalog()).Evaluate(AvailabilityExpression.When(new ContextComparison(path, op, operands)), Request());

    IEntitlements Requested()
    {
        _entitlementsRequested++;
        return _entitlements;
    }

    void Establish()
    {
        _entitlements = Substitute.For<IEntitlements>();
        _entitlements.WhyReadOnly().Returns(ReadOnlyReason.None);
        _entitlements.SeatCapacity().Returns(5);
        _entitlements.AccessTo(new ModuleId("analytics")).Returns(ModuleAccess.Full);
        _entitlements.AccessTo(new ModuleId("reports")).Returns(ModuleAccess.Preview);
        _entitlements.AccessTo(new ModuleId("forecasting")).Returns(ModuleAccess.Trial);
        _entitlements.AccessTo(new ModuleId("planning")).Returns(ModuleAccess.None);

        var offering = new ProductOfferingBuilder()
            .Product(ProductId.New())
            .Module("core", "Core", module => module.Availability(ModuleAvailability.Included).Owns("Acme.Core"))
            .Module("analytics", "Analytics", module => module.Owns("Acme.Analytics"))
            .Module("reports", "Reports", module => module.Availability(ModuleAvailability.Preview).Owns("Acme.Reports"))
            .Module("forecasting", "Forecasting", module => module.Owns("Acme.Forecasting"))
            .Module("planning", "Planning", module => module.Owns("Acme.Planning"))
            .Build();

        _host = new(Requested, offering);
    }
}
#endif
