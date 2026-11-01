// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.Notes.given;
using Cratis.Showrunner.Notes.Tenancy;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Notes.Transport.for_CollectorRelayWorker;

public class when_computing_the_delay : Specification
{
    CollectorRelayWorker _worker;

    void Establish()
    {
        var options = Options.Create(new NotesOptions { RelayInterval = TimeSpan.FromSeconds(30) });
        var tenants = new TenantEventStores(Substitute.For<IChronicleClient>(), Options.Create(new ChronicleClientOptions()));
        var collector = new given.RecordingCollector();
        _worker = new(
            tenants,
            new OutboxRelay(collector, options),
            new ResponseApplier(collector, tenants, options, new RecordingLogger<ResponseApplier>()),
            options,
            TimeProvider.System,
            new RecordingLogger<CollectorRelayWorker>());
    }

    [Fact] void should_use_the_interval_when_nothing_failed() => _worker.DelayAfter(0).ShouldEqual(TimeSpan.FromSeconds(30));
    [Fact] void should_double_it_for_each_failure() => _worker.DelayAfter(2).ShouldEqual(TimeSpan.FromMinutes(2));
    [Fact] void should_not_wait_longer_than_two_minutes() => _worker.DelayAfter(50).ShouldEqual(TimeSpan.FromMinutes(2));
}
#endif
