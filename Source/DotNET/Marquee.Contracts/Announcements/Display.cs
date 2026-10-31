// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Marquee.Contracts.Announcements;

/// <summary>
/// How an announcement is presented to the people it reaches.
/// </summary>
public enum Display
{
    /// <summary>
    /// A banner shown for as long as the announcement is active.
    /// </summary>
    Banner = 0,

    /// <summary>
    /// A dialog shown once per person; it does not come back after it is acknowledged.
    /// </summary>
    DialogOnce = 1,

    /// <summary>
    /// A dialog shown at the start of every session until the announcement ends.
    /// </summary>
    DialogEverySession = 2
}
