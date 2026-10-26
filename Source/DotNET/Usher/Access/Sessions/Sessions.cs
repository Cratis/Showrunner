// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Usher.Contracts.Conversation;
using Cratis.Showrunner.Usher.Identity;

namespace Cratis.Showrunner.Usher.Access.Sessions;

/// <summary>
/// Identifies one session in which support signed in as a reporter. The identifier is the one AuthProxy gives the
/// on-behalf session: an opaque value Usher never interprets, only records and compares.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record SupportSessionId(string Value) : ConceptAs<string>(Value)
{
    /// <summary>
    /// Represents an unset <see cref="SupportSessionId"/>.
    /// </summary>
    public static readonly SupportSessionId NotSet = new(string.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="SupportSessionId"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator SupportSessionId(string value) => new(value);
}

/// <summary>
/// Usher's grant check let AuthProxy start a session in which support signs in as the reporter: the grant was live and
/// the session had not started yet (Cratis/AuthProxy#217). AuthProxy checks the grant once before it announces a
/// session, so a session is permitted at most once; a later grant check for a session that was permitted but never
/// started is not live, so a session the application never recorded as started can not be used. It has no public
/// event.
/// </summary>
/// <param name="SessionId">The session.</param>
[EventType]
public record SupportSessionPermitted(SupportSessionId SessionId);

/// <summary>
/// Support signed in as the reporter under a live grant to look into the issue (Cratis/Showrunner#7). Both identities
/// are in the causation of this and every event appended in the session; the payload carries the one the reporter is
/// shown. It has no public event.
/// </summary>
/// <param name="SessionId">The session.</param>
/// <param name="Author">Who signed in, as the reporter is shown them.</param>
/// <param name="Actor">Who signed in, at their identity provider, as AuthProxy's verified assertion names them.</param>
[EventType]
public record SupportSignedInAsReporter(SupportSessionId SessionId, SupportAuthor Author, UserIdentity Actor);

/// <summary>
/// A session in which support signed in as the reporter ended, because support signed out, the grant was revoked or
/// it lapsed (Cratis/Showrunner#7). It is recorded once per session, however many times the end is reported. It has
/// no public event.
/// </summary>
/// <param name="SessionId">The session that ended.</param>
[EventType]
public record SupportSessionEnded(SupportSessionId SessionId);
