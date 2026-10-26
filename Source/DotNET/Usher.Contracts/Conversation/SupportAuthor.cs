// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.Conversation;

/// <summary>
/// Who wrote a support entry, as the reporter is meant to see it. It travels in the payload because the desk's own
/// causation (who appended the event in the desk store) does not cross into the application: the application
/// records the translated event under its own causation, and the desk decides what the reporter may see.
/// </summary>
/// <param name="Name">The author's name; <see cref="SupportAuthorName.NotSet"/> when the desk hides it.</param>
/// <param name="Kind">Whether the author is a person or an agent.</param>
public record SupportAuthor(SupportAuthorName Name, SupportAuthorKind Kind);
