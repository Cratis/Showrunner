// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;

namespace Cratis.Showrunner.Notes.Collecting;

/// <summary>
/// Represents how the builder of applications configures the collecting side of Notes.
/// </summary>
public class NoteCollectingOptions
{
    /// <summary>
    /// Gets or sets the names of the event stores of the applications whose outbox the notes come from. The contracts name
    /// no store, so nothing routes them on its own: the subscription is explicit. Applications that appear at run time,
    /// such as the ones the builder publishes, are named by an <see cref="IAppStores"/> the host supplies instead. Defaults
    /// to none.
    /// </summary>
    public IEnumerable<EventStoreName> AppStores { get; set; } = [];

    /// <summary>
    /// Gets or sets the names of the event stores of the desks that decide who may give feedback, for example <c>Direct</c>.
    /// The collecting side subscribes to the outbox of each for <c>FeedbackAccessGranted</c> and <c>FeedbackAccessRevoked</c>
    /// and records each decision in the namespace of the tenant it names. The collector serves what they leave granted to
    /// the applications that use the HTTP transport with only granted users allowed to give feedback. Defaults to none: a
    /// host whose applications share its kernel needs no desk here.
    /// </summary>
    public IEnumerable<EventStoreName> AccessStores { get; set; } = [];

    /// <summary>
    /// Gets or sets how often the collecting side opens the namespaces it does not serve yet and makes sure it is
    /// subscribed to the applications' stores. Everything it acts on is durable, so a round that fails is retried at the
    /// next one. Defaults to one minute.
    /// </summary>
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Gets or sets the event store the collecting side reads and records in. Defaults to <see cref="EventStoreName.NotSet"/>,
    /// which means the host's own event store: the one its Cratis or Chronicle setup registers. Set it only to use another one.
    /// </summary>
    public EventStoreName EventStore { get; set; } = EventStoreName.NotSet;
}
