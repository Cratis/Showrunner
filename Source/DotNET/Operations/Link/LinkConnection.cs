// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Link;

/// <summary>
/// How the link stood at the end of the last pass.
/// </summary>
public enum LinkConnection
{
    /// <summary>
    /// The link has not been enrolled: there is no instance credential and no enrollment token.
    /// </summary>
    NotEnrolled = 0,

    /// <summary>
    /// The claim was confirmed this pass.
    /// </summary>
    Connected = 1,

    /// <summary>
    /// Direct could not be reached; the watchdog ran on its local configuration and queued what it observed.
    /// </summary>
    Offline = 2,

    /// <summary>
    /// Direct refused the epoch or the session: another watchdog holds the instance and this one stopped acting through the link.
    /// </summary>
    Fenced = 3,

    /// <summary>
    /// Direct did not accept the credential or token.
    /// </summary>
    Unauthorized = 4,

    /// <summary>
    /// The link refused to operate: its saved state could not be read, or Direct presented a different signing key than the pinned one.
    /// </summary>
    Refused = 5,
}
