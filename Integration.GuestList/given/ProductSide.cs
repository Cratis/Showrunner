// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Registrations;
using Cratis.Showrunner.GuestList.Querying;
using Cratis.Showrunner.Usher.Integration.given;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.GuestList.Integration.given;

/// <summary>
/// The receiving side the builder of a product hosts: its own event store, with GuestList running, subscribed to the operator
/// store's outbox for the product. It never calls the operator: everything it answers is what it recorded.
/// </summary>
/// <param name="host">The running host.</param>
/// <param name="name">The name of the product store.</param>
public sealed class ProductSide(IHost host, EventStoreName name) : IAsyncDisposable
{
    /// <summary>
    /// The product the host is.
    /// </summary>
    public static readonly Product Product = "ledger";

    /// <summary>
    /// Gets the audiences the product knows, as the availability engine reads them.
    /// </summary>
    public IGuestList GuestList => host.Services.GetRequiredService<IGuestList>();

    /// <summary>
    /// Starts a product with a unique store name, receiving from an operator store.
    /// </summary>
    /// <param name="infrastructure">The kernel.</param>
    /// <param name="operatorStore">The operator store it receives from.</param>
    /// <returns>The running host.</returns>
    /// <exception cref="InvalidOperationException">The product store did not register.</exception>
    public static async Task<ProductSide> Start(ChronicleInfrastructure infrastructure, EventStoreName operatorStore)
    {
        var name = new EventStoreName($"Ledger-{Guid.NewGuid():N}");
        var builder = Host.CreateApplicationBuilder();
        builder.AddCratisChronicle(options =>
        {
            options.ConnectionString = new(infrastructure.ChronicleConnectionString);
            options.EventStore = name;
        });
        builder.AddGuestList(guestList =>
        {
            guestList.Product = Product;
            guestList.OperatorStores = [operatorStore];
            guestList.PollingInterval = TimeSpan.FromMilliseconds(500);
        });

        if (Environment.GetEnvironmentVariable("USHER_INTEGRATION_LOG") is { Length: > 0 } log)
        {
            builder.Logging.AddProvider(new FileLoggerProvider(log));
        }

        var host = builder.Build();
        await host.StartAsync();
        var product = new ProductSide(host, name);
        var registration = await (await product.In(EventStoreNamespaceName.Default)).WaitForRegistration(TimeSpan.FromSeconds(30));
        if (!registration.IsSuccess)
        {
            throw new InvalidOperationException($"Product store {name} did not register.", registration.Failure);
        }

        return product;
    }

    /// <summary>
    /// Gets the product store in a namespace. Opening a namespace is what gives the product that tenant: Chronicle forwards
    /// the operator's events only into namespaces the product has.
    /// </summary>
    /// <param name="namespace">The namespace.</param>
    /// <returns>The event store.</returns>
    public async Task<IEventStore> In(EventStoreNamespaceName @namespace)
    {
        var store = await host.Services.GetRequiredService<IChronicleClient>().GetEventStore(name, @namespace);
        await store.WaitForRegistration(TimeSpan.FromSeconds(30));
        return store;
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await host.StopAsync();
        host.Dispose();
    }
}
