// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using Cratis.Chronicle;
using Cratis.Chronicle.Testing.Events;
using Cratis.Showrunner.Notes.given;
using Cratis.Showrunner.Notes.Tenancy;
using Cratis.Showrunner.Notes.Transport.given;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Notes.Transport.for_CollectorRelayWorker;

/// <summary>
/// The host keeps running, the failure is logged, and the secret is nowhere in the log.
/// </summary>
public class when_the_collector_keeps_failing : Specification
{
    const string Secret = "s3cret-value-of-the-publication";

    EventStoreForTesting _eventStore;
    RecordingLogger<CollectorRelayWorker> _logger;
    CollectorRelayWorker _worker;
    RecordingCollector _collector;
    Exception _stopped;

    void Establish()
    {
        _eventStore = new();
        var root = Substitute.For<IEventStore>();
        root.GetNamespaces(Arg.Any<CancellationToken>()).Returns(_ => [EventStoreNamespaceName.Default]);
        var client = Substitute.For<IChronicleClient>();
        client.GetEventStore(Arg.Any<EventStoreName>(), Arg.Any<EventStoreNamespaceName?>()).Returns(_eventStore);
        var configured = new NotesOptions
        {
            Transport = NotesTransport.Http,
            CollectorToken = Secret,
            RelayInterval = TimeSpan.FromMilliseconds(1)
        };
        var options = Options.Create(configured);
        var tenants = new TenantEventStores(client, Options.Create(new ChronicleClientOptions { EventStore = "Ledger" }));
        _collector = new() { FailWith = new CollectorRequestFailed(HttpStatusCode.ServiceUnavailable) };
        _logger = new();
        _worker = new(
            tenants,
            new OutboxRelay(_collector, options),
            new ResponseApplier(_collector, tenants, options, new RecordingLogger<ResponseApplier>()),
            options,
            TimeProvider.System,
            _logger);
    }

    void Destroy() => _eventStore.Dispose();

    async Task Because()
    {
        await _worker.StartAsync(CancellationToken.None);
        for (var attempt = 0; attempt < 500 && _logger.Messages.Count == 0; attempt++)
        {
            await Task.Delay(10);
        }

        _stopped = await Record.ExceptionAsync(() => _worker.StopAsync(CancellationToken.None));
    }

    [Fact] void should_log_the_failure_as_a_warning() => _logger.Messages.First().Level.ShouldEqual(LogLevel.Warning);
    [Fact] void should_stop_cleanly() => _stopped.ShouldBeNull();
    [Fact] void should_not_log_the_secret() => _logger.Messages.Any(_ => _.Text.Contains(Secret, StringComparison.Ordinal)).ShouldBeFalse();
    [Fact] void should_wait_longer_after_each_failure() => _worker.DelayAfter(3).ShouldBeGreaterThan(_worker.DelayAfter(1));
}
#endif
