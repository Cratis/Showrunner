// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Usher.Contracts.Conversation;

namespace Cratis.Showrunner.Usher.Access.Sessions;

/// <summary>
/// What the issue's events say about one session in which support signed in as the reporter.
/// </summary>
/// <param name="Author">Who signed in.</param>
/// <param name="StartedAt">The sequence number of the event that started it.</param>
/// <param name="Ended">Whether it ended.</param>
public record SupportSessionFacts(SupportAuthor Author, EventSequenceNumber StartedAt, bool Ended);
