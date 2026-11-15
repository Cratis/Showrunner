// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Notes.Contracts.Access;

namespace Cratis.Showrunner.Notes.Access.Receiving;

/// <summary>
/// The desk granted the user the right to give feedback. It is translated from the public <c>FeedbackAccessGranted</c> and
/// recorded under the user's <see cref="FeedbackGiverId"/>, with the user as its compliance subject.
/// </summary>
/// <param name="GrantedBy">Who at the desk granted it.</param>
[EventType]
public record DeskGrantedFeedbackAccess(FeedbackGrantor GrantedBy);
