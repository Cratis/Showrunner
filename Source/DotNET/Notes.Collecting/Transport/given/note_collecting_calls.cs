// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Text;
using System.Text.Json;
using Cratis.Chronicle;
using Cratis.Chronicle.Testing.Events;
using Cratis.Showrunner.Notes.Collecting.Receiving;
using Cratis.Showrunner.Notes.Contracts.Transport;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Notes.Collecting.Transport.given;

/// <summary>
/// The calls of the HTTP transport over a collector with the tenant namespace <see cref="Tenant"/>, behind a host that knows
/// the publication <see cref="PublicationName"/> by the secret <see cref="Secret"/>.
/// </summary>
public class note_collecting_calls : Specification
{
    protected const string Secret = "s3cret-value-of-the-publication";
    protected const string PublicationName = "pub-7f3a";
    protected const string Tenant = "fjord-logistics";

    protected EventStoreForTesting _tenantStore;
    protected IResolveNotesPublications _publications;
    protected NoteCollectingCalls _calls;

    void Establish()
    {
        var root = Substitute.For<IEventStore>();
        root.GetNamespaces(Arg.Any<CancellationToken>()).Returns(_ => [EventStoreNamespaceName.Default, Tenant]);
        _tenantStore = new();
        var client = Substitute.For<IChronicleClient>();
        client.GetEventStore(Arg.Any<EventStoreName>(), Arg.Is<EventStoreNamespaceName?>(_ => _ == EventStoreNamespaceName.Default)).Returns(root);
        client.GetEventStore(Arg.Any<EventStoreName>(), Arg.Is<EventStoreNamespaceName?>(_ => _ == Tenant)).Returns(_tenantStore);
        _publications = Substitute.For<IResolveNotesPublications>();
        _publications.Resolve(Arg.Any<PublicationId>(), Arg.Any<string>()).Returns(Task.FromResult<NotesPublication?>(null));
        _publications.Resolve(new PublicationId(PublicationName), Secret).Returns(Task.FromResult<NotesPublication?>(new NotesPublication(Tenant, "Ledger")));
        var collector = new NoteCollector(
            client,
            Options.Create(new ChronicleClientOptions { EventStore = "Studio" }),
            new AppEventTranslator(client, new Collecting.given.RecordingLogger<AppEventTranslator>()),
            new Collecting.given.RecordingLogger<NoteCollector>());
        _calls = new(_publications, collector);
    }

    void Destroy() => _tenantStore.Dispose();

    /// <summary>
    /// Creates a request.
    /// </summary>
    /// <param name="authorization">The Authorization header, if any.</param>
    /// <param name="body">The body, if any.</param>
    /// <param name="query">The query string, if any.</param>
    /// <returns>The HTTP context.</returns>
    protected static DefaultHttpContext Request(string? authorization, string body = "", string query = "")
    {
        var http = new DefaultHttpContext();
        if (authorization is not null)
        {
            http.Request.Headers.Authorization = authorization;
        }

        http.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        http.Request.QueryString = new QueryString(query);
        return http;
    }

    /// <summary>
    /// Gets the status code a result answers.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <returns>The status code.</returns>
    protected static int StatusOf(IResult result) => (result as IStatusCodeHttpResult)?.StatusCode ?? 200;

    /// <summary>
    /// Creates the body of a request that relays one note about the context.
    /// </summary>
    /// <param name="tenant">The tenant the note names.</param>
    /// <returns>The JSON.</returns>
    protected static string NoteFor(string tenant)
    {
        var given = new NoteOnContextGiven(tenant, Collecting.given.ReceivedNotes.Application, Collecting.given.ReceivedNotes.Giver, Collecting.given.ReceivedNotes.Context, NoteKind.Problem, "The invoice is not sent.");
        var type = typeof(NoteOnContextGiven).GetEventType();
        var note = new PublishedNoteEvent(
            0,
            new(type.Id.Value, type.Generation.Value),
            DateTimeOffset.UnixEpoch,
            Guid.NewGuid().ToString(),
            JsonSerializer.SerializeToElement(given, NotesTransportJson.Options));
        return JsonSerializer.Serialize(new[] { note }, NotesTransportJson.Options);
    }
}
#endif
