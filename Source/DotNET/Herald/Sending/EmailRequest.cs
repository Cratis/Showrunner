// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;

namespace Cratis.Showrunner.Herald.Sending;

/// <summary>
/// Represents a request to send an email.
/// </summary>
/// <param name="Kind">The kind of email, which is the name of its template.</param>
/// <param name="About">Whatever the email is about, such as the invitation. The facts about the email are recorded against it.</param>
/// <param name="To">The address to send to.</param>
/// <param name="Values">The values to fill the email with, by the names its template declares. Dates are ISO 8601.</param>
public record EmailRequest(EmailKind Kind, EventSourceId About, string To, IReadOnlyDictionary<string, string> Values)
{
    /// <summary>
    /// Gets the name shown beside the address.
    /// </summary>
    public string ToName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the language to write the email in; null for the default language. A language that has no template falls back
    /// to the language without its region, then to the default language.
    /// </summary>
    public EmailLanguage? Language { get; init; }

    /// <summary>
    /// Gets the compliance subject the email's personal data belongs to, for erasure; null to use the event source.
    /// </summary>
    public Subject? Subject { get; init; }
}
