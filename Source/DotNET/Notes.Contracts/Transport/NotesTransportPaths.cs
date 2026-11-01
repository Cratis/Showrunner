// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Contracts.Transport;

/// <summary>
/// The paths of the HTTP transport between a published application and the collecting side. The application relays its
/// notes to the collector and polls it for the answers; the collector maps the same paths.
/// </summary>
public static class NotesTransportPaths
{
    /// <summary>
    /// The route template of the group every endpoint is mapped in; the publication is the one segment that varies.
    /// </summary>
    public const string Group = "/.cratis/notes/v1/publications/{publicationId}";

    /// <summary>
    /// The route, relative to the group, notes are posted to.
    /// </summary>
    public const string Notes = "notes";

    /// <summary>
    /// The route, relative to the group, the position of the stored notes is read from.
    /// </summary>
    public const string Position = "notes/position";

    /// <summary>
    /// The route, relative to the group, the answers are read from.
    /// </summary>
    public const string Responses = "responses";

    /// <summary>
    /// The name of the query parameter that names the event log of the application a note was relayed from, when the
    /// application has more than one tenant namespace. Absent, it is the empty source.
    /// </summary>
    public const string SourceParameter = "source";

    /// <summary>
    /// The name of the query parameter that holds the cursor to read the answers after.
    /// </summary>
    public const string AfterParameter = "after";

    /// <summary>
    /// Gets the path of the group for a publication.
    /// </summary>
    /// <param name="publicationId">The publication.</param>
    /// <returns>The path, without a trailing slash.</returns>
    public static string For(PublicationId publicationId) =>
        $"/.cratis/notes/v1/publications/{Uri.EscapeDataString(publicationId.Value)}";
}
