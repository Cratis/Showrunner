// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Access.Sessions;

/// <summary>
/// How the end of a session in which support signed in as the reporter was recorded.
/// </summary>
public enum SupportSessionEnd
{
    /// <summary>
    /// The end was recorded now.
    /// </summary>
    Ended = 0,

    /// <summary>
    /// The end was already recorded; nothing was recorded again.
    /// </summary>
    AlreadyEnded = 1,

    /// <summary>
    /// No such session started on the issue, so there is nothing to end.
    /// </summary>
    NeverStarted = 2
}
