// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Access;

/// <summary>
/// Who may give feedback with Notes.
/// </summary>
public enum FeedbackAccessMode
{
    /// <summary>
    /// Every signed-in user may give feedback. This is how Notes worked before access could be controlled.
    /// </summary>
    Everyone = 0,

    /// <summary>
    /// Only the users a desk granted the right to give feedback may, until it revokes it.
    /// </summary>
    GrantedUsers = 1
}
