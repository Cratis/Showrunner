// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Registrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.Usher.Integration.given;

/// <summary>
/// An application running Usher in a generic host, with its own event store and Usher's background work running.
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
    /// Starts an application with a unique store name, talking to a desk.
    /// </summary>
    /// <param name="infrastructure">The kernel.</param>
    /// <param name="deskStore">The desk store it talks to.</param>
    /// <returns>The running application.</returns>
    /// <exception cref="InvalidOperationException">The application store did not register.</exception>
    public static async Task<Application> Start(ChronicleInfrastructure infrastructure, EventStoreName deskStore)
    {
        var name = new EventStoreName($"Ledger-{Guid.NewGuid():N}");
        var builder = Host.CreateApplicationBuilder();
        builder.AddCratisChronicle(options =>
        {
            options.ConnectionString = new(infrastructure.ChronicleConnectionString);
            options.EventStore = name;
        });
        builder.AddUsher(usher =>
        {
            usher.ApplicationName = "Ledger";
            usher.DeskStores = [deskStore];
            usher.PollingInterval = TimeSpan.FromMilliseconds(500);
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
    /// Waits for an event on an issue in the event log of a tenant namespace.
    /// </summary>
    /// <typeparam name="TEvent">The private event.</typeparam>
    /// <param name="tenant">The tenant namespace.</param>
    /// <param name="issueId">The issue.</param>
    /// <returns>The event as the event log holds it.</returns>
    public async Task<AppendedEvent> WaitFor<TEvent>(EventStoreNamespaceName tenant, IssueId issueId)
    {
        var store = await In(tenant);
        return await Eventually.Get(
            async () => (await store.EventLog.GetFromSequenceNumber(EventSequenceNumber.First, issueId)).FirstOrDefault(_ => _.Content is TEvent),
            $"{typeof(TEvent).Name} for {issueId} in {name}'s event log in {tenant}");
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await host.StopAsync();
        host.Dispose();
    }
}
