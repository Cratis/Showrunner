// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Marquee.Contracts.Announcements;

/// <summary>
/// How much attention an announcement asks for.
/// </summary>
public enum Importance
{
    /// <summary>
    /// For information; nothing is required of the reader.
    /// </summary>
    Information = 0,

    /// <summary>
    /// Something the reader should be aware of, such as an upcoming change.
    /// </summary>
    Warning = 1,

    /// <summary>
    /// Something that affects the reader now, such as an outage.
    /// </summary>
    Critical = 2
}
