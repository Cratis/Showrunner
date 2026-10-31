// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.Compute;

/// <summary>
/// How often an allowance renews.
/// </summary>
public enum AllowancePeriod
{
    /// <summary>
    /// The allowance renews every day.
    /// </summary>
    Day = 0,

    /// <summary>
    /// The allowance renews every month.
    /// </summary>
    Month = 1
}
