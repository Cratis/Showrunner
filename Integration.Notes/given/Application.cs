// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Registrations;
using Cratis.Showrunner.Notes.Access;
using Cratis.Showrunner.Notes.Contracts.Access;
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
    /// <param name="accessStore">The desk that decides who may give feedback; when given, only users it granted may.</param>
    /// <param name="kind">Whether the application runs published or staged.</param>
    /// <returns>The running application.</returns>
    /// <exception cref="InvalidOperationException">The application store did not register.</exception>
    public static async Task<Application> Start(ChronicleInfrastructure infrastructure, EventStoreName collectingStore, EventStoreName? accessStore = null, ApplicationKind kind = ApplicationKind.Published)
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
            notes.ApplicationKind = kind;
            if (accessStore is { } desk)
            {
                notes.AccessMode = FeedbackAccessMode.GrantedUsers;
                notes.AccessStores = [desk];
            }
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
    /// Starts an application that runs its own kernel, so it relays to a collector over HTTP, and lets only the users a desk
    /// granted give feedback, as the collector tells it.
    /// </summary>
    /// <param name="infrastructure">The kernel.</param>
    /// <param name="collectorUrl">The address of the collector.</param>
    /// <param name="secret">The secret the collector knows the publication by.</param>
    /// <param name="tenant">The tenant the collector knows the publication's notes by.</param>
    /// <returns>The running application.</returns>
    /// <exception cref="InvalidOperationException">The application store did not register.</exception>
    public static async Task<Application> StartOverHttp(ChronicleInfrastructure infrastructure, string collectorUrl, string secret, TenantId tenant)
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
            notes.PollingInterval = TimeSpan.FromMilliseconds(500);
            notes.Transport = NotesTransport.Http;
            notes.CollectorUrl = collectorUrl;
            notes.CollectorToken = secret;
            notes.CollectorTenantId = tenant;
            notes.RelayInterval = TimeSpan.FromMilliseconds(500);
            notes.AccessMode = FeedbackAccessMode.GrantedUsers;
            notes.AccessRefreshInterval = TimeSpan.FromMilliseconds(500);
            notes.AccessMaxStaleness = TimeSpan.FromMinutes(1);
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
    /// Gets the services of the running application.
    /// </summary>
    public IServiceProvider Services => host.Services;

    /// <summary>
    /// Gets the application store in a namespace, as a request in that tenant would.
    /// </summary>
    /// <param name="namespace">The namespace.</param>
    /// <returns>The event store.</returns>
    public Task<IEventStore> In(EventStoreNamespaceName @namespace) =>
        host.Services.GetRequiredService<IChronicleClient>().GetEventStore(name, @namespace);

    /// <summary>
    /// Waits for an event on a user's right to give feedback in the event log of a tenant namespace.
    /// </summary>
    /// <typeparam name="TEvent">The private event.</typeparam>
    /// <param name="tenant">The tenant namespace.</param>
    /// <param name="giver">The user.</param>
    /// <returns>The event as the event log holds it.</returns>
    public async Task<AppendedEvent> WaitFor<TEvent>(EventStoreNamespaceName tenant, FeedbackGiverIdentity giver)
    {
        var store = await In(tenant);
        var id = FeedbackGiverId.For(giver);
        return await Eventually.Get(
            async () => (await store.EventLog.GetFromSequenceNumber(EventSequenceNumber.First, id)).FirstOrDefault(_ => _.Content is TEvent),
            $"{typeof(TEvent).Name} for {giver.Subject.Value} in {name}'s event log in {tenant}");
    }

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
