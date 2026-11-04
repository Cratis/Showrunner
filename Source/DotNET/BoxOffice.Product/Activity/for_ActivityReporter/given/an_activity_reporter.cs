// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Collections.Immutable;
using Cratis.Chronicle.EventSequences;
using Cratis.Chronicle.Testing.Events;
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Product.Tenancy;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Product.Activity.for_ActivityReporter.given;

public class an_activity_reporter : Specification
{
    protected EventStoreForTesting _eventStore;
    protected ITenantStores _stores;
    protected Clock _clock;
    protected ActivityReporter _reporter;
    protected IImmutableList<AppendedEvent> _outbox;

    void Establish()
    {
        _eventStore = new();
        _stores = Substitute.For<ITenantStores>();
        _stores.For(Tenants.Tenant, Arg.Any<CancellationToken>()).Returns(_eventStore);
        _clock = new(Tenants.Now);
        _reporter = Reporter();
    }

    void Destroy() => _eventStore.Dispose();

    protected ActivityReporter Reporter() => new(_stores, _clock, Microsoft.Extensions.Logging.Abstractions.NullLogger<ActivityReporter>.Instance);

    protected Task OrganizationIsKnown() => _eventStore.EventLog.Append(TenantStream.Id, new TenantOrganizationIdentified(Tenants.Organization));

    protected async Task ReadOutbox() =>
        _outbox = await _eventStore.GetEventSequence(EventSequenceId.Outbox).GetFromSequenceNumber(EventSequenceNumber.First);
}
#endif
