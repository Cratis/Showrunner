// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;
using Cratis.Showrunner.Notes.Contracts.Responses;
using Cratis.Showrunner.Notes.Contracts.Transport;
using Cratis.Showrunner.Notes.Receiving;
using Cratis.Showrunner.Notes.Tenancy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Notes.Transport;

/// <summary>
/// Polls the collector for what the receiving side did with the application's notes and records it as the same private
/// <c>Receiver…</c> facts the same-kernel transport records, in the namespace the note was given in.
/// </summary>
/// <remarks>
/// <para>
/// There is no stored cursor. The application starts from the beginning of the collector's answers and keeps the cursor in
/// memory while it runs; a restart reads the answers again. That is safe because each answer is appended with an
/// idempotency key made of the publication and the answer's cursor: an answer that was applied is found in the note's
/// event stream and appends nothing, however often it is delivered. The key, and not a comparison with the note's current
/// state, decides: a note that was addressed, then declined, then addressed again must not be turned back to addressed
/// when the first answer is read once more.
/// </para>
/// <para>
/// The namespace is found from the note itself - the one whose event log holds the note being given - because the answer
/// names the note and nothing else. An answer for a note no namespace has is logged and skipped.
/// </para>
/// </remarks>
/// <param name="collector">The collector.</param>
/// <param name="tenants">Opens the tenant namespaces.</param>
/// <param name="options">Notes' options.</param>
/// <param name="logger">The logger.</param>
public class ResponseApplier(ICollectorClient collector, TenantEventStores tenants, IOptions<NotesOptions> options, ILogger<ResponseApplier> logger)
{
    /// <summary>
    /// Gets the cursor of the last answer applied.
    /// </summary>
    public ulong Cursor { get; private set; }

    /// <summary>
    /// Derives the idempotency key of an answer.
    /// </summary>
    /// <param name="publicationId">The publication.</param>
    /// <param name="cursor">The cursor of the answer.</param>
    /// <returns>The key.</returns>
    public static IdempotencyKey KeyFor(PublicationId publicationId, ulong cursor) =>
        new($"notes-http-response-{publicationId.Value}-{cursor}");

    /// <summary>
    /// Applies every answer the collector has after the cursor.
    /// </summary>
    /// <param name="cancellationToken">Stops the round when the host shuts down.</param>
    /// <returns>The number of answers read.</returns>
    public async Task<int> Apply(CancellationToken cancellationToken = default)
    {
        var read = 0;
        IReadOnlyList<IEventStore>? stores = null;
        while (true)
        {
            var responses = await collector.GetResponses(Cursor, cancellationToken);
            var unseen = responses.Where(_ => _.Cursor > Cursor).OrderBy(_ => _.Cursor).ToArray();
            if (unseen.Length == 0)
            {
                return read;
            }

            stores ??= await tenants.All(cancellationToken);
            foreach (var response in unseen)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await ApplyOne(response, stores);
                Cursor = response.Cursor;
                read++;
            }
        }
    }

    static async Task<IEventStore?> FindTenant(NoteResponse response, IReadOnlyList<IEventStore> stores)
    {
        foreach (var store in stores)
        {
            var openers = ReceiverEventTranslator.NoteOpeners.Select(store.EventTypes.GetEventTypeFor).ToArray();
            var opened = await store.EventLog.GetForEventSourceIdAndEventTypes(response.NoteId.ToString(), openers);
            if (opened.Count > 0)
            {
                return store;
            }
        }

        return null;
    }

    async Task ApplyOne(NoteResponse response, IReadOnlyList<IEventStore> stores)
    {
        var tenant = await FindTenant(response, stores);
        if (tenant is not { } store)
        {
            logger.UnknownNote(response.NoteId, response.Cursor);
            return;
        }

        var tenantId = new TenantId(store.Namespace.Value);
        object? publicEvent = response.Kind switch
        {
            NoteResponseKind.Acknowledged => new NoteAcknowledged(tenantId),
            NoteResponseKind.Addressed when !string.IsNullOrWhiteSpace(response.Version) => new NoteAddressed(tenantId, response.Version),
            NoteResponseKind.Declined when !string.IsNullOrWhiteSpace(response.Reason) => new NoteDeclined(tenantId, response.Reason),
            _ => null
        };

        if (publicEvent is null || PrivateEvents.For(publicEvent) is not { } translated)
        {
            logger.UnusableResponse(response.NoteId, response.Cursor, response.Kind);
            return;
        }

        var result = await store.EventLog.AppendOnce(
            new EventSourceId(response.NoteId.ToString()),
            translated.Fact,
            KeyFor(options.Value.PublicationId, response.Cursor),
            occurred: response.Occurred);

        if (result.WasAlreadyAppended)
        {
            logger.AlreadyApplied(response.NoteId, response.Cursor);
            return;
        }

        if (!result.IsSuccess)
        {
            throw new ReceiverFactNotRecorded(translated.Fact.GetType(), response.NoteId.ToString(), store.Namespace, result);
        }
    }
}
