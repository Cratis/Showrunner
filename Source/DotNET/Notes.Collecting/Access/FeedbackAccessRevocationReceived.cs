// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Notes.Contracts.Access;

namespace Cratis.Showrunner.Notes.Collecting.Access;

/// <summary>
/// The desk took the right to give feedback away from a user of the tenant. It is translated from the public
/// <c>FeedbackAccessRevoked</c> and recorded in the tenant's namespace under the user's <see cref="FeedbackGiverId"/>, with
/// the user as its compliance subject.
/// </summary>
/// <param name="RevokedBy">Who at the desk revoked it.</param>
[EventType]
public record FeedbackAccessRevocationReceived(FeedbackGrantor RevokedBy);
