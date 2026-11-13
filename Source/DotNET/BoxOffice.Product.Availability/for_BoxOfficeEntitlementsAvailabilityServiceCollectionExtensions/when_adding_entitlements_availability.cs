// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Choreography.Availability.Context;
using Cratis.Choreography.Availability.Context.Providers;
using Cratis.Showrunner.BoxOffice.Entitlements;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Showrunner.BoxOffice.Product.Availability.for_BoxOfficeEntitlementsAvailabilityServiceCollectionExtensions;

/// <summary>
/// Adding entitlements availability supplies the value from the scope of the request being answered.
/// </summary>
public class when_adding_entitlements_availability : Specification
{
    ServiceProvider _provider;
    IServiceProvider _requestServices;
    IHostContextValues _host;
    ContextValueOutcome _outcome;
    Exception _withoutRequest;

    void Establish()
    {
        var entitlements = Substitute.For<IEntitlements>();
        entitlements.SeatCapacity().Returns(7);
        entitlements.WhyReadOnly().Returns(ReadOnlyReason.None);
        _requestServices = new ServiceCollection().AddSingleton(entitlements).BuildServiceProvider();

        ServiceCollection services = [];
        services.AddBoxOfficeEntitlementsAvailability();
        _provider = services.BuildServiceProvider();
        _host = _provider.GetRequiredService<IHostContextValues>();
    }

    async Task Because()
    {
        _provider.GetRequiredService<IHttpContextAccessor>().HttpContext = new DefaultHttpContext { RequestServices = _requestServices };
        _outcome = await _host.Get("entitlements", CancellationToken.None);
        _provider.GetRequiredService<IHttpContextAccessor>().HttpContext = null;
        _withoutRequest = await Catch.Exception(async () => await _host.Get("entitlements", CancellationToken.None));
    }

    void Destroy() => _provider.Dispose();

    [Fact] void should_supply_the_entitlements_of_the_request() => ((EntitlementsValue)_outcome.Value!).Seats.ShouldEqual(7);
    [Fact] void should_use_the_entitlements_host_values() => _host.ShouldBeOfExactType<EntitlementsHostContextValues>();
    [Fact] void should_fail_loudly_without_a_request() => _withoutRequest.ShouldBeOfExactType<InvalidOperationException>();
}
#endif
