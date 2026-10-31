// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Cratis.Chronicle;
using Cratis.Chronicle.ReadModels;
using Cratis.Showrunner.Marquee.Receiving;
using Microsoft.Extensions.Options;
using ReactiveSubject = System.Reactive.Subjects.Subject;

namespace Cratis.Showrunner.Marquee.Standing;

/// <summary>
/// Represents an implementation of <see cref="IStandingAnnouncements"/> that reads the read model in the product's own
/// namespace, whichever tenant asks.
/// </summary>
/// <param name="client">The Chronicle client.</param>
/// <param name="options">The Chronicle client options, which name the product's event store.</param>
public class ProductWideAnnouncements(IChronicleClient client, IOptions<ChronicleClientOptions> options) : IStandingAnnouncements
{
    /// <inheritdoc/>
    public ISubject<IEnumerable<StandingAnnouncement>> Observe()
    {
        // The observation underneath is opened when the first subscriber arrives and released when the last one leaves.
        var observed = Observable.FromAsync(ProductStore).SelectMany(store => store.ReadModels.Observe<StandingAnnouncement>());
        return ReactiveSubject.Create<IEnumerable<StandingAnnouncement>>(Observer.Create<IEnumerable<StandingAnnouncement>>(_ => { }), observed);
    }

    /// <inheritdoc/>
    public async Task<StandingAnnouncement?> Find(AnnouncementId announcementId, CancellationToken cancellationToken = default)
    {
        var store = await ProductStore(cancellationToken);
        return await store.ReadModels.GetInstanceById<StandingAnnouncement>(new ReadModelKey(announcementId.Value.ToString()));
    }

    Task<IEventStore> ProductStore(CancellationToken cancellationToken) =>
        client.GetEventStore(options.Value.EventStore, MarqueeStream.Namespace).WaitAsync(cancellationToken);
}
