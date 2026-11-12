// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Choreography.Availability.Audiences;
using Cratis.Chronicle;
using Cratis.Chronicle.ReadModels;
using Cratis.Showrunner.GuestList.Contracts.Audiences;
using Cratis.Showrunner.GuestList.Holding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.GuestList.Availability.for_GuestListAvailabilityServiceCollectionExtensions;

/// <summary>
/// Adding GuestList availability wires the engine to the local read models. The engine answers from what the product holds
/// and never reaches for the operator, so the last known state stays in force while the link to the operator is down.
/// </summary>
public class when_adding_guest_list_availability : Specification
{
    static readonly EventStoreName ProductStore = new("Ledger");
    static readonly EventStoreName OperatorStore = new("Direct");

    ServiceProvider _provider;
    IChronicleClient _client;
    EffectiveAudiences _effective;
    IAudienceSource _source;
    IDeclaredAudiences _declared;
    IEnumerable<IHostedService> _hosted;

    void Establish()
    {
        var holdings = new AudienceHoldings(
            GuestListStreams.Tenant.Value,
            Cratis.Showrunner.GuestList.Contracts.Audiences.AudienceLevel.Tenant,
            [new AudienceDecision(new("Plus"), true, new(2026, 1, 5, 12, 0, 0, TimeSpan.Zero)), new AudienceDecision(new("Beta"), true, new(2026, 1, 5, 12, 0, 0, TimeSpan.Zero))]);
        var readModels = Substitute.For<IReadModels>();
        readModels.GetInstanceById<AudienceHoldings>(Arg.Any<ReadModelKey>(), Arg.Any<ReadModelSessionId>()).Returns(Task.FromResult(holdings));
        var store = Substitute.For<IEventStore>();
        store.ReadModels.Returns(readModels);
        _client = Substitute.For<IChronicleClient>();
        _client.GetEventStore(ProductStore, Arg.Any<EventStoreNamespaceName?>()).Returns(store);

        ServiceCollection services = [];
        services.AddLogging();
        services.AddSingleton(_client);
        services.Configure<ChronicleClientOptions>(options => options.EventStore = ProductStore);
        services.AddGuestList(options =>
        {
            options.Product = new("Ledger");
            options.OperatorStores = [OperatorStore];
            options.DeclaredAudiences = [new("Plus")];
        });
        services.AddGuestListAvailability();
        _provider = services.BuildServiceProvider();
    }

    async Task Because()
    {
        _source = _provider.GetRequiredService<IAudienceSource>();
        _declared = _provider.GetRequiredService<IDeclaredAudiences>();
        _hosted = _provider.GetServices<IHostedService>();
        _effective = await _provider.GetRequiredService<IAudienceResolver>().Resolve(new("acme", null));
    }

    void Destroy() => _provider.Dispose();

    [Fact] void should_use_guest_list_as_the_audience_source() => _source.ShouldBeOfExactType<GuestListAudienceSource>();
    [Fact] void should_declare_what_guest_list_is_told_the_application_declares() => _declared.All.ShouldContainOnly(new Cratis.Choreography.Availability.Audiences.AudienceName("Plus"));
    [Fact] void should_resolve_the_declared_audience_the_tenant_holds() => _effective.Holds("Plus").ShouldBeTrue();
    [Fact] void should_carry_but_not_match_what_is_not_declared() => _effective.Undeclared.Select(_ => _.Audience.Value).Distinct().ShouldContainOnly("Beta");
    [Fact] void should_watch_for_changes_in_the_background() => _hosted.OfType<AudienceChangeWatcher>().ShouldNotBeEmpty();
    [Fact] void should_never_ask_the_operator() => _client.DidNotReceive().GetEventStore(OperatorStore, Arg.Any<EventStoreNamespaceName?>());
    [Fact] void should_register_the_resolver_of_the_engine() => _provider.GetService<IAudienceResolver>().ShouldNotBeNull();
}
#endif
