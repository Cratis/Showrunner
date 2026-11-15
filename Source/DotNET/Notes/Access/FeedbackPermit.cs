// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Access;

/// <summary>
/// The proof that the signed-in user may give feedback now. A command that gives feedback gets one from
/// <see cref="IFeedbackAccess.Permit"/> and gives the note as the permit's giver.
/// </summary>
/// <param name="Giver">The user who may give feedback.</param>
public record FeedbackPermit(NoteGiver Giver);
