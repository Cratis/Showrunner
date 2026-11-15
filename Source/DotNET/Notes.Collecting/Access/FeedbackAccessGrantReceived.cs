// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Notes.Contracts.Access;

namespace Cratis.Showrunner.Notes.Collecting.Access;

/// <summary>
/// The desk granted a user of the tenant the right to give feedback. It is translated from the public
/// <c>FeedbackAccessGranted</c> and recorded in the tenant's namespace under the user's <see cref="FeedbackGiverId"/>, with
/// the user as its compliance subject. The collector hands the users it holds as granted to the applications that run their
/// own kernel.
/// </summary>
/// <param name="GrantedBy">Who at the desk granted it.</param>
[EventType]
public record FeedbackAccessGrantReceived(FeedbackGrantor GrantedBy);
