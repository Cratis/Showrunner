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

public class a_product_that_does_not_provision_tenants : Specification
{
    IChronicleClient _client;
    ServiceProvider _provider;

    async Task Because()
    {
        _client = Substitute.For<IChronicleClient>();
        _provider = new ServiceCollection().BuildServiceProvider();
        var options = Options.Create(new BoxOfficeOptions());
        var subscriptions = new ProvisioningSubscriptions(_provider.GetRequiredService<IServiceScopeFactory>(), options);
        var tenants = new TenantEventStores(_client, Options.Create(new ChronicleClientOptions()), BoxOfficeEventStore.ForClientOptions(Options.Create(new ChronicleClientOptions()), Options.Create(new OfferingOptions { EventStore = "Ledger" })));
        var worker = new BoxOfficeProductWorker(tenants, subscriptions, _provider.GetRequiredService<IServiceProviderIsService>(), options, TimeProvider.System, Microsoft.Extensions.Logging.Abstractions.NullLogger<BoxOfficeProductWorker>.Instance);
        await worker.RunOnce();
    }

    void Destroy() => _provider.Dispose();

    [Fact] void should_not_reach_for_the_event_store() => _client.ReceivedCalls().ShouldBeEmpty();
}
#endif
