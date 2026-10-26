// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Issues;

/// <summary>
/// How an issue came to be reported.
/// </summary>
public enum IssueKind
{
    /// <summary>
    /// The reporter reported an error they hit.
    /// </summary>
    Error = 0,

    /// <summary>
    /// The reporter described a problem by hand.
    /// </summary>
    Manual = 1
}
