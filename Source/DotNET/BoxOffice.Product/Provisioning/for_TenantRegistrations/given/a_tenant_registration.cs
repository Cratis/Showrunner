// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Collections.Immutable;
using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Chronicle.Testing.Events;
using Cratis.Execution;
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Product.Provisioning.for_TenantRegistrations.given;

public class a_tenant_registration : Specification
{
    protected static readonly TenantRegistered Registered = new(Tenants.Tenant, "Fjord Logistics", Tenants.Ada, "Maintainer");

    protected EventStoreForTesting _eventStore;
    protected IChronicleClient _client;
    protected IProvisionTenants _provisioner;
    protected IClosingTenants _closing;
    protected TenantRegistrations _registrations;
    protected EventContext _context;
    protected IImmutableList<AppendedEvent> _outbox;
    protected IImmutableList<AppendedEvent> _tenantEvents;

    void Establish()
    {
        _eventStore = new();
        _client = Substitute.For<IChronicleClient>();
        _client.GetEventStore(Arg.Any<EventStoreName>(), Arg.Any<EventStoreNamespaceName?>()).Returns(_eventStore);
        _provisioner = Substitute.For<IProvisionTenants>();
        _provisioner.Provision(Arg.Any<TenantToProvision>(), Arg.Any<CancellationToken>()).Returns(TenantProvisioning.Provisioned);
        _closing = Substitute.For<IClosingTenants>();
        _registrations = new(_client, _provisioner, Microsoft.Extensions.Logging.Abstractions.NullLogger<TenantRegistrations>.Instance, _closing);
        _context = EventContext.From(
            "Direct",
            EventStoreNamespaceName.Default,
            EventType.Unknown,
            EventSourceType.Default,
            Tenants.Organization.Value.ToString("D"),
            EventStreamType.All,
            EventStreamId.Default,
            0,
            CorrelationId.New(),
            Tenants.Start);
    }

    void Destroy() => _eventStore.Dispose();

    protected async Task Read()
    {
        _outbox = await _eventStore.GetEventSequence(EventSequenceId.Outbox).GetFromSequenceNumber(EventSequenceNumber.First);
        _tenantEvents = await _eventStore.EventLog.GetFromSequenceNumber(EventSequenceNumber.First);
    }
}
#endif
