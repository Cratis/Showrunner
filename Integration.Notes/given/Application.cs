// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Registrations;
using Cratis.Showrunner.Usher.Integration.given;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.Notes.Integration.given;

/// <summary>
/// An application running Notes in a generic host, with its own event store and Notes' background work running.
/// </summary>
/// <param name="host">The running host.</param>
/// <param name="name">The name of the application store.</param>
public sealed class Application(IHost host, EventStoreName name) : IAsyncDisposable
{
    /// <summary>
    /// Gets the name of the application store.
    /// </summary>
    public EventStoreName Name => name;

    /// <summary>
    /// Starts an application with a unique store name, talking to a collecting store.
    /// </summary>
    /// <param name="infrastructure">The kernel.</param>
    /// <param name="collectingStore">The collecting store it talks to.</param>
    /// <returns>The running application.</returns>
    /// <exception cref="InvalidOperationException">The application store did not register.</exception>
    public static async Task<Application> Start(ChronicleInfrastructure infrastructure, EventStoreName collectingStore)
    {
        var name = new EventStoreName($"Ledger-{Guid.NewGuid():N}");
        var builder = Host.CreateApplicationBuilder();
        builder.AddCratisChronicle(options =>
        {
            options.ConnectionString = new(infrastructure.ChronicleConnectionString);
            options.EventStore = name;
        });
        builder.AddNotes(notes =>
        {
            notes.ApplicationName = "Ledger";
            notes.PublicationId = "pub-7f3a";
            notes.ApplicationVersion = "1.2.3";
            notes.CollectingStores = [collectingStore];
            notes.PollingInterval = TimeSpan.FromMilliseconds(500);
        });

        if (Environment.GetEnvironmentVariable("USHER_INTEGRATION_LOG") is { Length: > 0 } log)
        {
            builder.Logging.AddProvider(new FileLoggerProvider(log));
        }

        var host = builder.Build();
        await host.StartAsync();
        var application = new Application(host, name);
        var registration = await (await application.In(EventStoreNamespaceName.Default)).WaitForRegistration(TimeSpan.FromSeconds(30));
        if (!registration.IsSuccess)
        {
            throw new InvalidOperationException($"Application store {name} did not register.", registration.Failure);
        }

        return application;
    }

    /// <summary>
    /// Gets the application store in a namespace, as a request in that tenant would.
    /// </summary>
    /// <param name="namespace">The namespace.</param>
    /// <returns>The event store.</returns>
    public Task<IEventStore> In(EventStoreNamespaceName @namespace) =>
        host.Services.GetRequiredService<IChronicleClient>().GetEventStore(name, @namespace);

    /// <summary>
    /// Waits for an event on a note in the event log of a tenant namespace.
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

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await host.StopAsync();
        host.Dispose();
    }
}
