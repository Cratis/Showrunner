// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Chronicle.ReadModels;
using Cratis.Chronicle.Testing.Events;
using Cratis.Chronicle.Transactions;
using Cratis.Showrunner.BoxOffice.Compute;
using Cratis.Showrunner.BoxOffice.Gating;
using Cratis.Showrunner.BoxOffice.Offering.Declaration;
using Cratis.Showrunner.BoxOffice.Offering.Metering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.BoxOffice.for_BoxOfficeServiceCollectionExtensions.when_adding_box_office;

public class for_a_product_that_declares_an_offering_and_meters : Specification
{
    EventStoreForTesting _eventStore;
    ServiceProvider _provider;
    IModuleGate _gate;
    IComputeAdmission _admission;

    void Establish()
    {
        _eventStore = new();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IEventStore>(_eventStore);
        services.AddSingleton(Substitute.For<IReadModels>());
        services.AddSingleton(Substitute.For<IUnitOfWorkManager>());
        services.AddSingleton(Substitute.For<IMeteringStore>());
        services.AddSingleton(new ProductOfferingBuilder().Product(ProductId.New()).Meter("agent-runs", "Agent runs", "runs").Build());
        services.AddBoxOffice();
        _provider = services.BuildServiceProvider();
    }

    void Because()
    {
        using var scope = _provider.CreateScope();
        _gate = scope.ServiceProvider.GetRequiredService<IModuleGate>();
        _admission = scope.ServiceProvider.GetRequiredService<IComputeAdmission>();
    }

    void Destroy()
    {
        _provider.Dispose();
        _eventStore.Dispose();
    }

    [Fact] void should_provide_the_module_gate() => _gate.ShouldBeOfExactType<ModuleGate>();
    [Fact] void should_provide_compute_admission() => _admission.ShouldBeOfExactType<TenantComputeAdmission>();
}
#endif
