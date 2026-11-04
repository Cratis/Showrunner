// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.BoxOffice.Offering;
using Cratis.Showrunner.BoxOffice.Offering.Declaration;
using Cratis.Showrunner.BoxOffice.Product.Provisioning;
using Cratis.Showrunner.BoxOffice.Tenancy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice.Product.for_BoxOfficeProductWorker.when_running;

public class a_product_that_provisions_tenants : Specification
{
    IEventStore _root;
    IChronicleClient _client;
    ServiceProvider _provider;
    bool _registered;

    async Task Because()
    {
        _root = Substitute.For<IEventStore>();
        _root.GetNamespaces(Arg.Any<CancellationToken>()).Returns(Task.FromResult<IEnumerable<EventStoreNamespaceName>>([]));
        _root.Reactors.Returns(Substitute.For<Cratis.Chronicle.Reactors.IReactors>());
        _client = Substitute.For<IChronicleClient>();
        _client.GetEventStore(Arg.Any<EventStoreName>(), Arg.Any<EventStoreNamespaceName?>()).Returns(_root);

        var services = new ServiceCollection();
        services.AddSingleton(Substitute.For<IProvisionTenants>());
        _provider = services.BuildServiceProvider();
        var options = Options.Create(new BoxOfficeOptions());
        var subscriptions = new ProvisioningSubscriptions(_provider.GetRequiredService<IServiceScopeFactory>(), options);
        var tenants = new TenantEventStores(_client, Options.Create(new ChronicleClientOptions()), BoxOfficeEventStore.ForClientOptions(Options.Create(new ChronicleClientOptions()), Options.Create(new OfferingOptions { EventStore = "Ledger" })));
        var worker = new BoxOfficeProductWorker(tenants, subscriptions, _provider.GetRequiredService<IServiceProviderIsService>(), options, TimeProvider.System, Microsoft.Extensions.Logging.Abstractions.NullLogger<BoxOfficeProductWorker>.Instance);
        await worker.RunOnce();
        _registered = _root.Reactors.ReceivedCalls().Any();
    }

    void Destroy() => _provider.Dispose();

    [Fact] void should_listen_for_the_tenants_the_operator_registers() => _registered.ShouldBeTrue();
}
#endif
