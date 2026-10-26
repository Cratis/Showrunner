// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Access.Sessions;

/// <summary>
/// Why support may not act as a reporter in a session.
/// </summary>
public enum SupportSessionRefusal
{
    /// <summary>
    /// The issue was never reported.
    /// </summary>
    IssueNotReported = 0,

    /// <summary>
    /// The user support wants to act as did not report the issue.
    /// </summary>
    NotTheReporter = 1,

    /// <summary>
    /// The actor is the reporter: nobody acts on behalf of themselves.
    /// </summary>
    ActorIsTheReporter = 2,

    /// <summary>
    /// No grant is in effect: it was never given, it was revoked, it lapsed or its end has passed.
    /// </summary>
    NoLiveGrant = 3,

    /// <summary>
    /// The reporter did not let support know who they are, so there is nobody to act as.
    /// </summary>
    IdentityNotShared = 4,

    /// <summary>
    /// The session already ended; an ended session never resumes.
    /// </summary>
    SessionEnded = 5,

    /// <summary>
    /// The issue's access kept changing while the session was being decided.
    /// </summary>
    Contended = 6,

    /// <summary>
    /// The reporter withdrew the issue, which ends support's access to it for good.
    /// </summary>
    IssueWithdrawn = 7
}
