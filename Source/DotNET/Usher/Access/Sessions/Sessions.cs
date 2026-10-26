// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Usher.Contracts.Conversation;

namespace Cratis.Showrunner.Usher.Access.Sessions;

/// <summary>
/// Identifies one session in which support signed in as a reporter.
/// </summary>
/// <param name="Value">The underlying value.</param>
public record SupportSessionId(Guid Value) : ConceptAs<Guid>(Value), IGeneratable<SupportSessionId>
{
    /// <summary>
    /// Represents an unset <see cref="SupportSessionId"/>.
    /// </summary>
    public static readonly SupportSessionId NotSet = new(Guid.Empty);

    /// <summary>
    /// Implicitly converts a <see cref="Guid"/> to a <see cref="SupportSessionId"/>.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    public static implicit operator SupportSessionId(Guid value) => new(value);

    /// <summary>
    /// Creates a new <see cref="SupportSessionId"/>.
    /// </summary>
    /// <returns>A new <see cref="SupportSessionId"/>.</returns>
    public static SupportSessionId New() => new(GenerateValue.Uuid());
}

/// <summary>
/// Support signed in as the reporter under a live grant to look into the issue (Cratis/Showrunner#7). Both identities
/// are in the causation of this and every event appended in the session; the payload carries the one the reporter is
/// shown. It has no public event.
/// </summary>
/// <param name="SessionId">The session.</param>
/// <param name="Author">Who signed in, as the reporter is shown them.</param>
[EventType]
public record SupportSignedInAsReporter(SupportSessionId SessionId, SupportAuthor Author);

/// <summary>
/// A session in which support signed in as the reporter ended, because support signed out, the grant was revoked or
/// it lapsed (Cratis/Showrunner#7). It has no public event.
/// </summary>
/// <param name="SessionId">The session that ended.</param>
[EventType]
public record SupportSessionEnded(SupportSessionId SessionId);
