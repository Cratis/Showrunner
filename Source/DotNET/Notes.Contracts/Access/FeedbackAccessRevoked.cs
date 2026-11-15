// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Contracts.Access;

/// <summary>
/// Published by a desk when it takes the right to give feedback with Notes away from one user of one application
/// tenant. The latest of <see cref="FeedbackAccessGranted"/> and this event for a user decides whether they may give
/// feedback. Notes the user gave before stay theirs to see and withdraw.
/// </summary>
/// <param name="TenantId">The tenant the user belongs to.</param>
/// <param name="Giver">The user who loses the right.</param>
/// <param name="RevokedBy">Who at the desk revoked it.</param>
[EventType]
[Public]
public record FeedbackAccessRevoked(TenantId TenantId, FeedbackGiverIdentity Giver, FeedbackGrantor RevokedBy);
