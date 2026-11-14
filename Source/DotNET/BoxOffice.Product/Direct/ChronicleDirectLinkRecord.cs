// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Cratis.Showrunner.BoxOffice.Offering;

namespace Cratis.Showrunner.BoxOffice.Product.Direct;

/// <summary>
/// Records the facts of the link in the event log of the product's own event store, like Showrunner records its other
/// facts. The last fact on the link's stream is its state.
/// </summary>
/// <param name="client">The <see cref="IChronicleClient"/> to find the store with.</param>
/// <param name="eventStore">The <see cref="BoxOfficeEventStore"/> that names the product's event store.</param>
public class ChronicleDirectLinkRecord(IChronicleClient client, BoxOfficeEventStore eventStore) : IDirectLinkRecord
{
    /// <summary>
    /// The event source id of the link's stream.
    /// </summary>
    public const string Stream = "direct-link";

    /// <inheritdoc/>
    public async Task<DirectLinkState> Read(CancellationToken cancellationToken)
    {
        var store = await Store(cancellationToken);
        var types = new[] { typeof(DirectLinkRedeemed), typeof(DirectLinkVerified), typeof(DirectLinkReopened) }.Select(store.EventTypes.GetEventTypeFor).ToArray();
        var facts = await store.EventLog.GetForEventSourceIdAndEventTypes(Stream, types);
        return facts.Count == 0
            ? new(DirectLinkStage.NotConfigured, string.Empty)
            : facts[^1].Content switch
            {
                DirectLinkRedeemed redeemed => new(DirectLinkStage.Redeemed, redeemed.Endpoint),
                DirectLinkVerified verified => new(DirectLinkStage.Closed, verified.Endpoint),
                DirectLinkReopened => new(DirectLinkStage.NotConfigured, string.Empty),
                _ => new(DirectLinkStage.NotConfigured, string.Empty)
            };
    }

    /// <inheritdoc/>
    public Task Redeemed(string endpoint, CancellationToken cancellationToken) => Append(new DirectLinkRedeemed(endpoint), cancellationToken);

    /// <inheritdoc/>
    public async Task Verified(CancellationToken cancellationToken) =>
        await Append(new DirectLinkVerified((await Read(cancellationToken)).Endpoint), cancellationToken);

    /// <inheritdoc/>
    public async Task Reopened(CancellationToken cancellationToken) =>
        await Append(new DirectLinkReopened((await Read(cancellationToken)).Endpoint), cancellationToken);

    async Task<IEventStore> Store(CancellationToken cancellationToken) =>
        await client.GetEventStore(eventStore.Name, EventStoreNamespaceName.Default).WaitAsync(cancellationToken);

    async Task Append(object @event, CancellationToken cancellationToken)
    {
        var store = await Store(cancellationToken);
        var result = await store.EventLog.Append(Stream, @event);
        if (!result.IsSuccess)
        {
            throw new InvalidOperationException("The state of the link to Chronicle could not be recorded.");
        }
    }
}
