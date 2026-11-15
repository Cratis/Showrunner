// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Contracts.Access;

/// <summary>
/// Published by a desk when it grants one user of one application tenant the right to give feedback with Notes. The
/// grant stands until <see cref="FeedbackAccessRevoked"/> for the same user; granting again after a revocation grants
/// again. It only matters to an application that lets only granted users give feedback.
/// </summary>
/// <param name="TenantId">The tenant the user belongs to.</param>
/// <param name="Giver">The user who is granted the right.</param>
/// <param name="GrantedBy">Who at the desk granted it.</param>
[EventType]
[Public]
public record FeedbackAccessGranted(TenantId TenantId, FeedbackGiverIdentity Giver, FeedbackGrantor GrantedBy);
