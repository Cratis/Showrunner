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
}
