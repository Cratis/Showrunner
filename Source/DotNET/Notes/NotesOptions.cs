// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;

namespace Cratis.Showrunner.Notes;

/// <summary>
/// Represents how an application configures Notes.
/// </summary>
public class NotesOptions
{
    /// <summary>
    /// Gets or sets whether Notes is switched on. When it is off every command is refused, the availability query says
    /// so and the feedback button does not render. Defaults to <see langword="true"/>.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the name of the application, as the people building it see it on every note. Required.
    /// </summary>
    public ApplicationName ApplicationName { get; set; } = ApplicationName.NotSet;

    /// <summary>
    /// Gets or sets the publication the application runs as: how the receiving side finds the organization, application
    /// and event model a note is about. The builder of the application assigns it when it publishes the application.
    /// Required.
    /// </summary>
    public PublicationId PublicationId { get; set; } = PublicationId.NotSet;

    /// <summary>
    /// Gets or sets the version of the application, published with every note. Defaults to
    /// <see cref="ApplicationVersion.NotSet"/>, which means the informational version of the entry assembly, without its
    /// build metadata (the part after <c>+</c>).
    /// </summary>
    public ApplicationVersion ApplicationVersion { get; set; } = ApplicationVersion.NotSet;

    /// <summary>
    /// Gets or sets the names of the event stores of the receiving sides the application publishes its notes to and
    /// receives answers from. The contracts name no store, so nothing routes them on its own: the subscription is explicit.
    /// Defaults to <c>Studio</c>.
    /// </summary>
    public IEnumerable<EventStoreName> CollectingStores { get; set; } = ["Studio"];

    /// <summary>
    /// Gets or sets how often Notes looks for tenant namespaces it does not serve yet and makes sure the application is
    /// subscribed to the receiving stores. Everything it acts on is durable, so a round that fails is retried at the next
    /// one. Defaults to one minute.
    /// </summary>
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Gets or sets how the application moves its notes to the receiving side and gets the answers back.
    /// <see cref="NotesTransport.SameKernel"/>, the default, uses Chronicle's outbox, subscription and inbox, which needs
    /// the application and the receiving side to share a Chronicle kernel. <see cref="NotesTransport.Http"/> relays over
    /// HTTP to <see cref="CollectorUrl"/>, for an application that runs its own kernel.
    /// </summary>
    public NotesTransport Transport { get; set; } = NotesTransport.SameKernel;

    /// <summary>
    /// Gets or sets the absolute URL of the collector, the base the <c>/.cratis/notes/v1/…</c> endpoints sit under. Required
    /// when <see cref="Transport"/> is <see cref="NotesTransport.Http"/>; ignored otherwise.
    /// </summary>
    public string CollectorUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the bearer secret the collector knows the publication by. Prefer <see cref="CollectorTokenFile"/>: a
    /// value in configuration is easy to leak. It is never logged. Used when <see cref="Transport"/> is <see cref="NotesTransport.Http"/>.
    /// </summary>
    public string CollectorToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the path of a file, typically a mounted secret, that holds the bearer secret. The file is read again
    /// whenever the secret is used, so a rotated secret takes effect without a restart. It wins over <see cref="CollectorToken"/>.
    /// </summary>
    public string CollectorTokenFile { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tenant the collector knows the publication's notes by. When set, every note the application
    /// publishes names it as its <c>TenantId</c> instead of the name of the namespace it was given in, because the
    /// collector rejects a note whose tenant is not the one its publication names. Leave it unset when the namespace is
    /// named like the tenant. Used when <see cref="Transport"/> is <see cref="NotesTransport.Http"/>.
    /// </summary>
    public TenantId CollectorTenantId { get; set; } = TenantId.NotSet;

    /// <summary>
    /// Gets or sets how often the relay looks for notes to send and answers to apply. A round that fails is retried after
    /// a growing delay. Defaults to five seconds. Used when <see cref="Transport"/> is <see cref="NotesTransport.Http"/>.
    /// </summary>
    public TimeSpan RelayInterval { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Gets or sets the most notes sent in one request. Defaults to 100. Used when <see cref="Transport"/> is <see cref="NotesTransport.Http"/>.
    /// </summary>
    public int RelayBatchSize { get; set; } = 100;

    /// <summary>
    /// Gets or sets the event store Notes records the application's facts in. Defaults to <see cref="EventStoreName.NotSet"/>,
    /// which means the application's own event store: the one its Cratis or Chronicle setup registers. Set it only to use
    /// another one.
    /// </summary>
    public EventStoreName EventStore { get; set; } = EventStoreName.NotSet;
}
