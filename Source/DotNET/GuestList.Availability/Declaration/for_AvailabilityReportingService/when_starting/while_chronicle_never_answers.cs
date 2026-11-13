// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.GuestList.Availability.Declaration.for_AvailabilityReporter.given;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Manifest = Cratis.Choreography.Availability;

namespace Cratis.Showrunner.GuestList.Availability.Declaration.for_AvailabilityReportingService.when_starting;

/// <summary>
/// Chronicle accepts the connection and never answers: starting the host and stopping it both complete.
/// </summary>
public class while_chronicle_never_answers : a_product_reporting
{
    bool _started;
    bool _stopped;

    async Task Because()
    {
        var client = Substitute.For<IChronicleClient>();
        client
            .GetEventStore(Arg.Any<EventStoreName>(), Arg.Any<EventStoreNamespaceName?>())
            .Returns(_ => new TaskCompletionSource<IEventStore>().Task);

        var options = Options.Create(new GuestListOptions { Product = "ledger", Environment = "Staging", Version = "1.0.0" });
        var service = new AvailabilityReportingService(
            client,
            Options.Create(new ChronicleClientOptions { EventStore = "Ledger" }),
            new(new Manifest.InMemoryAvailabilityManifest(PlusAndPreview), options, new Manifest.AvailabilityOptions(), NullLogger<AvailabilityReporter>.Instance),
            options,
            TimeProvider.System,
            NullLogger<AvailabilityReportingService>.Instance);

        var start = service.StartAsync(CancellationToken.None);
        _started = await Task.WhenAny(start, Task.Delay(TimeSpan.FromSeconds(5))) == start;
        var stop = service.StopAsync(CancellationToken.None);
        _stopped = await Task.WhenAny(stop, Task.Delay(TimeSpan.FromSeconds(5))) == stop;
        await ReadReported();
    }

    [Fact] void should_start() => _started.ShouldBeTrue();
    [Fact] void should_stop() => _stopped.ShouldBeTrue();
    [Fact] void should_report_nothing() => _reported.Count.ShouldEqual(0);
}
#endif
