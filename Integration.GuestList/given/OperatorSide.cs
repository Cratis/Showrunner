// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Registrations;
using Cratis.Showrunner.GuestList.Authoring.Decisions;
using Cratis.Showrunner.Usher.Integration.given;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.GuestList.Integration.given;

/// <summary>
/// The operator side the builder of a product hosts: its own event store, with GuestList's authoring outbox running. It has
/// no Direct and no web host; what an operator's commands record is appended to its event log, and the outbox publishes it.
/// </summary>
/// <param name="host">The running host.</param>
/// <param name="name">The name of the operator store.</param>
public sealed class OperatorSide(IHost host, EventStoreName name) : IAsyncDisposable
{
    /// <summary>
    /// Gets the name of the operator store.
    /// </summary>
    public EventStoreName Name => name;

    /// <summary>
    /// Starts an operator side with a unique store name.
    /// </summary>
    /// <param name="infrastructure">The kernel.</param>
    /// <returns>The running host.</returns>
    /// <exception cref="InvalidOperationException">The operator store did not register.</exception>
    public static async Task<OperatorSide> Start(ChronicleInfrastructure infrastructure)
    {
        var name = new EventStoreName($"Direct-{Guid.NewGuid():N}");
        var builder = Host.CreateApplicationBuilder();
        builder.AddCratisChronicle(options =>
        {
            options.ConnectionString = new(infrastructure.ChronicleConnectionString);
            options.EventStore = name;
        });
        builder.Services.AddGuestListAuthoring();

        if (Environment.GetEnvironmentVariable("USHER_INTEGRATION_LOG") is { Length: > 0 } log)
        {
            builder.Logging.AddProvider(new FileLoggerProvider(log));
        }

        var host = builder.Build();
        await host.StartAsync();
        var operatorSide = new OperatorSide(host, name);
        var registration = await (await operatorSide.In(EventStoreNamespaceName.Default)).WaitForRegistration(TimeSpan.FromSeconds(30));
        if (!registration.IsSuccess)
        {
            throw new InvalidOperationException($"Operator store {name} did not register.", registration.Failure);
        }

        return operatorSide;
    }

    /// <summary>
    /// Gets the operator store in a namespace.
    /// </summary>
    /// <param name="namespace">The namespace.</param>
    /// <returns>The event store.</returns>
    public Task<IEventStore> In(EventStoreNamespaceName @namespace) =>
        host.Services.GetRequiredService<IChronicleClient>().GetEventStore(name, @namespace);

    /// <summary>
    /// Records what the operator's commands record: an audience was decided, in the stream of the product.
    /// </summary>
    /// <param name="product">The product.</param>
    /// <param name="decisions">The decisions: <see cref="AudienceGrantDecided"/> and <see cref="AudienceWithdrawalDecided"/>.</param>
    /// <returns>An awaitable task.</returns>
    public async Task Decide(Product product, params object[] decisions)
    {
        var store = await In(EventStoreNamespaceName.Default);
        foreach (var decision in decisions)
        {
            await store.EventLog.Append(new EventSourceId(product.Value), decision);
        }
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await host.StopAsync();
        host.Dispose();
    }
}
