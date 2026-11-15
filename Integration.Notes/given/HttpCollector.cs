// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Cryptography;
using System.Text;
using Cratis.Chronicle.Registrations;
using Cratis.Showrunner.Notes.Collecting.Transport;
using Cratis.Showrunner.Notes.Contracts.Applications;
using Cratis.Showrunner.Usher.Integration.given;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.Notes.Integration.given;

/// <summary>
/// The receiving side the builder of an application hosts, reachable over HTTP: its own event store, with Notes.Collecting's
/// background work running and the collector's endpoints mapped on a loopback address, subscribed to a desk that decides who
/// may give feedback. It knows one publication by one secret.
/// </summary>
/// <param name="app">The running web application.</param>
/// <param name="name">The name of the collecting store.</param>
/// <param name="url">The address the endpoints are served on.</param>
public sealed class HttpCollector(WebApplication app, EventStoreName name, string url) : IAsyncDisposable
{
    /// <summary>
    /// Gets the name of the collecting store.
    /// </summary>
    public EventStoreName Name => name;

    /// <summary>
    /// Gets the address the collector's endpoints are served on.
    /// </summary>
    public string Url => url;

    /// <summary>
    /// Starts a collecting host with a unique store name that listens on a free loopback port.
    /// </summary>
    /// <param name="infrastructure">The kernel.</param>
    /// <param name="accessStore">The desk whose decisions the collector holds.</param>
    /// <param name="publication">The publication the collector authenticates.</param>
    /// <param name="publicationId">The id of the publication.</param>
    /// <param name="secret">The secret the publication presents.</param>
    /// <returns>The running host.</returns>
    /// <exception cref="InvalidOperationException">The collecting store did not register.</exception>
    public static async Task<HttpCollector> Start(ChronicleInfrastructure infrastructure, EventStoreName accessStore, NotesPublication publication, PublicationId publicationId, string secret)
    {
        var name = new EventStoreName($"Studio-{Guid.NewGuid():N}");
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.AddCratisChronicle(options =>
        {
            options.ConnectionString = new(infrastructure.ChronicleConnectionString);
            options.EventStore = name;
        });
        builder.AddNoteCollecting(collecting =>
        {
            collecting.AccessStores = [accessStore];
            collecting.PollingInterval = TimeSpan.FromMilliseconds(500);
        });
        builder.Services.AddSingleton<IResolveNotesPublications>(new FixedPublication(publicationId, secret, publication));

        if (Environment.GetEnvironmentVariable("USHER_INTEGRATION_LOG") is { Length: > 0 } log)
        {
            builder.Logging.AddProvider(new FileLoggerProvider(log));
        }

        var app = builder.Build();
        app.MapNoteCollecting();
        await app.StartAsync();
        var collector = new HttpCollector(app, name, app.Urls.First());
        var registration = await (await collector.In(EventStoreNamespaceName.Default)).WaitForRegistration(TimeSpan.FromSeconds(30));
        if (!registration.IsSuccess)
        {
            throw new InvalidOperationException($"Collecting store {name} did not register.", registration.Failure);
        }

        return collector;
    }

    /// <summary>
    /// Gets the collecting store in a namespace. Opening a namespace is what gives the store that tenant: Chronicle
    /// forwards a desk's events only into namespaces the collecting store has.
    /// </summary>
    /// <param name="namespace">The namespace.</param>
    /// <returns>The event store.</returns>
    public async Task<IEventStore> In(EventStoreNamespaceName @namespace)
    {
        var store = await app.Services.GetRequiredService<IChronicleClient>().GetEventStore(name, @namespace);
        await store.WaitForRegistration(TimeSpan.FromSeconds(30));
        return store;
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await app.StopAsync();
        await app.DisposeAsync();
    }

    sealed class FixedPublication(PublicationId id, string secret, NotesPublication publication) : IResolveNotesPublications
    {
        public Task<NotesPublication?> Resolve(PublicationId publicationId, string bearerToken)
        {
            var matches = publicationId == id && CryptographicOperations.FixedTimeEquals(Hash(bearerToken), Hash(secret));
            return Task.FromResult<NotesPublication?>(matches ? publication : null);
        }

        static byte[] Hash(string value) => SHA256.HashData(Encoding.UTF8.GetBytes(value));
    }
}
