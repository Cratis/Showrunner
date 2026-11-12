// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Choreography.Availability.Audiences;
using Cratis.Chronicle;
using Cratis.Chronicle.ReadModels;
using Cratis.Showrunner.GuestList.Availability.given;
using Cratis.Showrunner.GuestList.Holding;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.GuestList.Availability.for_AudienceChangeWatcher.given;

/// <summary>
/// An <see cref="AudienceChangeWatcher"/> over a product event store whose default namespace and tenant namespaces each have a
/// read model a specification can change.
/// </summary>
public class a_watcher : Specification
{
    protected static readonly EventStoreName ProductStore = new("Ledger");

    protected readonly Dictionary<EventStoreNamespaceName, WatchedHoldings> _watched = [];
    protected readonly List<AudienceChange> _heard = [];
    protected List<EventStoreNamespaceName> _tenants;
    protected IChronicleClient _client;
    protected a_clock _clock;
    protected AudienceChangeFeed _feed;
    protected AudienceChangeWatcher _watcher;

    void Establish()
    {
        _tenants = [new("acme")];
        _clock = new();
        _feed = new(_clock, NullLogger<AudienceChangeFeed>.Instance);
        _feed.Subscribe(change =>
        {
            _heard.Add(change);
            return ValueTask.CompletedTask;
        });

        _client = Substitute.For<IChronicleClient>();
        _client.GetEventStore(ProductStore, Arg.Any<EventStoreNamespaceName?>()).Returns(call => Task.FromResult(StoreOf(call.ArgAt<EventStoreNamespaceName?>(1) ?? EventStoreNamespaceName.Default)));

        var chronicle = Options.Create(new ChronicleClientOptions { EventStore = ProductStore });
        var metrics = new GuestListAvailabilityMetrics(new TestMeterFactory(), _feed);
        _watcher = new(_client, chronicle, _feed, metrics, Options.Create(new GuestListOptions()), _clock, NullLogger<AudienceChangeWatcher>.Instance);
    }

    IEventStore StoreOf(EventStoreNamespaceName @namespace)
    {
        if (!_watched.TryGetValue(@namespace, out var holdings))
        {
            holdings = _watched[@namespace] = new();
        }

        var store = Substitute.For<IEventStore>();
        var readModels = Substitute.For<IReadModels>();
        readModels.Watch<AudienceHoldings>().Returns(holdings);
        store.ReadModels.Returns(readModels);
        store.GetNamespaces(Arg.Any<CancellationToken>()).Returns(_ => Task.FromResult<IEnumerable<EventStoreNamespaceName>>(_tenants));
        return store;
    }

    /// <summary>
    /// Changes the holding in a namespace as the read model does when a decision is recorded.
    /// </summary>
    /// <param name="namespace">The namespace.</param>
    /// <param name="holdingsId">The stream of the holding.</param>
    protected void Changed(EventStoreNamespaceName @namespace, string holdingsId) =>
        _watched[@namespace].Emit(new(@namespace, new ReadModelKey(holdingsId), null, false));

    /// <summary>
    /// A read model whose changes a specification raises, and whose subscribers it counts.
    /// </summary>
    public class WatchedHoldings : IObservable<ReadModelChangeset<AudienceHoldings>>
    {
        readonly List<IObserver<ReadModelChangeset<AudienceHoldings>>> _observers = [];

        /// <summary>
        /// Gets how many times the read model was subscribed to.
        /// </summary>
        public int Subscriptions { get; private set; }

        /// <inheritdoc/>
        public IDisposable Subscribe(IObserver<ReadModelChangeset<AudienceHoldings>> observer)
        {
            Subscriptions++;
            _observers.Add(observer);
            return new Unsubscribe(() => _observers.Remove(observer));
        }

        /// <summary>
        /// Raises a change.
        /// </summary>
        /// <param name="changeset">The change.</param>
        public void Emit(ReadModelChangeset<AudienceHoldings> changeset)
        {
            foreach (var observer in _observers.ToArray())
            {
                observer.OnNext(changeset);
            }
        }

        /// <summary>
        /// Ends the watch with an error, as a dropped connection does.
        /// </summary>
        public void Fail()
        {
            foreach (var observer in _observers.ToArray())
            {
                observer.OnError(new InvalidOperationException("The connection to Chronicle dropped."));
            }
        }

        sealed class Unsubscribe(Action dispose) : IDisposable
        {
            public void Dispose() => dispose();
        }
    }

    sealed class TestMeterFactory : System.Diagnostics.Metrics.IMeterFactory
    {
        public System.Diagnostics.Metrics.Meter Create(System.Diagnostics.Metrics.MeterOptions options) => new(options);

        public void Dispose()
        {
        }
    }
}
#endif
