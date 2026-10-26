// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Access.Sessions;

/// <summary>
/// A session in which support acts as the reporter right now: the one issue it is for, under a live grant.
/// </summary>
/// <param name="IssueId">The issue the session is for; the only issue support may see or act on in it.</param>
/// <param name="SessionId">The session.</param>
public record LiveSupportSession(IssueId IssueId, SupportSessionId SessionId);
