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
/// Chronicle cannot be reached when the host starts: the host is not held back, the service tries again, and reports
/// once Chronicle answers.
/// </summary>
public class while_chronicle_is_down : a_product_reporting
{
    AvailabilityReportingService _service;
    bool _startedPromptly;
    int _attempts;

    async Task Because()
    {
        var client = Substitute.For<IChronicleClient>();
        client
            .GetEventStore(Arg.Any<EventStoreName>(), Arg.Any<EventStoreNamespaceName?>())
            .Returns(_ => ++_attempts <= 2 ? Task.FromException<IEventStore>(new InvalidOperationException("Chronicle is down")) : Task.FromResult<IEventStore>(_eventStore));

        var options = Options.Create(new GuestListOptions { Product = "ledger", Environment = "Staging", Version = "1.0.0", RetryInterval = TimeSpan.FromMilliseconds(10) });
        _service = new(
            client,
            Options.Create(new ChronicleClientOptions { EventStore = "Ledger" }),
            new(new Manifest.InMemoryAvailabilityManifest(PlusAndPreview), options, new Manifest.AvailabilityOptions(), NullLogger<AvailabilityReporter>.Instance),
            options,
            TimeProvider.System,
            NullLogger<AvailabilityReportingService>.Instance);

        var start = _service.StartAsync(CancellationToken.None);
        _startedPromptly = await Task.WhenAny(start, Task.Delay(TimeSpan.FromSeconds(5))) == start;
        var until = DateTime.UtcNow.AddSeconds(10);
        do
        {
            await ReadReported();
            if (_reported.Count > 0)
            {
                break;
            }

            await Task.Delay(20);
        }
        while (DateTime.UtcNow < until);

        await _service.StopAsync(CancellationToken.None);
    }

    [Fact] void should_not_hold_the_host_back() => _startedPromptly.ShouldBeTrue();
    [Fact] void should_try_again_until_chronicle_answers() => _attempts.ShouldEqual(3);
    [Fact] void should_report_once_chronicle_answers() => _reported.Count.ShouldEqual(1);
}
#endif
