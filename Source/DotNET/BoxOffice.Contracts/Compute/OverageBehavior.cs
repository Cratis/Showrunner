// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.Compute;

/// <summary>
/// What the product does when an allowance is used up.
/// </summary>
public enum OverageBehavior
{
    /// <summary>
    /// New work is refused until the allowance renews or is raised.
    /// </summary>
    Block = 0,

    /// <summary>
    /// New work waits until the allowance renews or is raised.
    /// </summary>
    Park = 1
}
