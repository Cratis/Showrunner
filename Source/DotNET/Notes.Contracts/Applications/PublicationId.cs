// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Contracts.Applications;

/// <summary>
/// Identifies the publication an application runs as, so the receiving side can find the organization, application and event model a note is about. The builder of the application assigns it when it publishes the application.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record PublicationId(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="PublicationId"/>.
    /// </summary>
    public static readonly PublicationId NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="PublicationId"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator PublicationId(string value) => new(value);
}
