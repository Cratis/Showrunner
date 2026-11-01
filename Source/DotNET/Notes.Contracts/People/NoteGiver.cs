// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Contracts.People;

/// <summary>
/// Who gave a note: the signed-in user of the application, as the application's identity tells it. It is always taken
/// from the authenticated identity, never from what a client sends.
/// </summary>
/// <param name="UserId">The stable id of the user at their identity provider; personal data.</param>
/// <param name="DisplayName">The user's display name; personal data, <see cref="GiverDisplayName.NotSet"/> when the identity gave none.</param>
public record NoteGiver(GiverUserId UserId, GiverDisplayName DisplayName);
