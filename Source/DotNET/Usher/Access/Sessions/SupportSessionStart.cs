// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Access.Sessions;

/// <summary>
/// How a session in which support signs in as the reporter was recorded as started.
/// </summary>
public enum SupportSessionStart
{
    /// <summary>
    /// The start was recorded now.
    /// </summary>
    Started = 0,

    /// <summary>
    /// The start was already recorded; nothing was recorded again.
    /// </summary>
    AlreadyStarted = 1
}
