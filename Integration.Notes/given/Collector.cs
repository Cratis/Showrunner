// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Registrations;
using Cratis.Showrunner.Usher.Integration.given;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.Notes.Integration.given;

/// <summary>
/// The receiving side the builder of an application hosts: its own event store, with Notes.Collecting's background work
/// running, reading the application's outbox into its inbox.
/// </summary>
/// <param name="host">The running host.</param>
/// <param name="name">The name of the collecting store.</param>
public sealed class Collector(IHost host, EventStoreName name) : IAsyncDisposable
{
    /// <summary>
    /// Gets the name of the collecting store.
    /// </summary>
    public EventStoreName Name => name;

    /// <summary>
    /// Starts a collecting host with a unique store name, collecting from an application store.
    /// </summary>
    /// <param name="infrastructure">The kernel.</param>
    /// <param name="name">The name of the collecting store.</param>
    /// <param name="applicationStore">The application store it collects from.</param>
    /// <returns>The running host.</returns>
    /// <exception cref="InvalidOperationException">The collecting store did not register.</exception>
    public static async Task<Collector> Start(ChronicleInfrastructure infrastructure, EventStoreName name, EventStoreName applicationStore)
    {
        var builder = Host.CreateApplicationBuilder();
        builder.AddCratisChronicle(options =>
        {
            options.ConnectionString = new(infrastructure.ChronicleConnectionString);
            options.EventStore = name;
        });
        builder.AddNoteCollecting(collecting =>
        {
            collecting.AppStores = [applicationStore];
            collecting.PollingInterval = TimeSpan.FromMilliseconds(500);
        });

        if (Environment.GetEnvironmentVariable("USHER_INTEGRATION_LOG") is { Length: > 0 } log)
        {
            builder.Logging.AddProvider(new FileLoggerProvider(log));
        }

        var host = builder.Build();
        await host.StartAsync();
        var collector = new Collector(host, name);
        var registration = await (await collector.In(EventStoreNamespaceName.Default)).WaitForRegistration(TimeSpan.FromSeconds(30));
        if (!registration.IsSuccess)
        {
            throw new InvalidOperationException($"Collecting store {name} did not register.", registration.Failure);
        }

        return collector;
    }

    /// <summary>
    /// Gets the collecting store in a namespace. Opening a namespace is what gives the store that tenant: Chronicle
    /// forwards the application's events only into namespaces the collecting store has.
    /// </summary>
    /// <param name="namespace">The namespace.</param>
    /// <returns>The event store.</returns>
    public async Task<IEventStore> In(EventStoreNamespaceName @namespace)
    {
        var store = await host.Services.GetRequiredService<IChronicleClient>().GetEventStore(name, @namespace);
        await store.WaitForRegistration(TimeSpan.FromSeconds(30));
        return store;
    }

    /// <summary>
    /// Waits for a fact on a note in the event log of a tenant namespace.
    /// </summary>
    /// <typeparam name="TEvent">The private event.</typeparam>
    /// <param name="tenant">The tenant namespace.</param>
    /// <param name="noteId">The note.</param>
    /// <returns>The event as the event log holds it.</returns>
    public async Task<AppendedEvent> WaitFor<TEvent>(EventStoreNamespaceName tenant, NoteId noteId)
    {
        var store = await In(tenant);
        return await Eventually.Get(
            async () => (await store.EventLog.GetFromSequenceNumber(EventSequenceNumber.First, noteId)).FirstOrDefault(_ => _.Content is TEvent),
            $"{typeof(TEvent).Name} for {noteId} in {name}'s event log in {tenant}");
    }

    /// <summary>
    /// Gets everything recorded on a note in the event log of a tenant namespace.
    /// </summary>
    /// <param name="tenant">The tenant namespace.</param>
    /// <param name="noteId">The note.</param>
    /// <returns>The events, in the order they were recorded.</returns>
    public async Task<IReadOnlyList<AppendedEvent>> Recorded(EventStoreNamespaceName tenant, NoteId noteId) =>
        await (await In(tenant)).EventLog.GetFromSequenceNumber(EventSequenceNumber.First, noteId);

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await host.StopAsync();
        host.Dispose();
    }
}
