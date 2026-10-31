// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Compute;

/// <summary>
/// What was decided about a unit of work that asked to start.
/// </summary>
public enum AdmissionOutcome
{
    /// <summary>
    /// The tenant may run the work now.
    /// </summary>
    Admitted = 0,

    /// <summary>
    /// The tenant already runs as many units of work of the meter at the same time as it may.
    /// </summary>
    AtConcurrencyLimit = 1,

    /// <summary>
    /// The tenant used all the operator allows for the period, and what happens to work now is <see cref="OverageBehavior.Block"/>.
    /// </summary>
    AllowanceExhausted = 2,

    /// <summary>
    /// The tenant used all the product's maintainers budgeted for the period, and what happens to work now is <see cref="OverageBehavior.Block"/>.
    /// </summary>
    BudgetReached = 3,

    /// <summary>
    /// The allowance or the budget is used up and the work waits until the period renews. <see cref="AdmissionDecision.ParkedUntil"/>
    /// says when, and <see cref="AdmissionDecision.Because"/> says which of the two it hit.
    /// </summary>
    Parked = 4
}
