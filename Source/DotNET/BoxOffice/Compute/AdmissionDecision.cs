// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Compute;

/// <summary>
/// The decision on a unit of work that asked to start.
/// </summary>
/// <param name="Outcome">What was decided.</param>
/// <param name="Work">The unit of work to end when the work stops; only an admitted unit of work that counts against a
/// concurrency limit has one.</param>
/// <param name="ParkedUntil">When the period renews and the work may be tried again; set only when parked.</param>
/// <param name="Because">Which of <see cref="AdmissionOutcome.AllowanceExhausted"/> and <see cref="AdmissionOutcome.BudgetReached"/> parked the work; set only when parked.</param>
public record AdmissionDecision(AdmissionOutcome Outcome, ComputeWork? Work, DateTimeOffset? ParkedUntil, AdmissionOutcome? Because)
{
    /// <summary>
    /// Gets whether the work may start.
    /// </summary>
    public bool IsAdmitted => Outcome == AdmissionOutcome.Admitted;

    /// <summary>
    /// Creates the decision to admit the work.
    /// </summary>
    /// <param name="work">The unit of work to end when it stops, when it counts against a concurrency limit.</param>
    /// <returns>The decision.</returns>
    public static AdmissionDecision Admitted(ComputeWork? work = null) => new(AdmissionOutcome.Admitted, work, null, null);

    /// <summary>
    /// Creates the decision to turn the work away.
    /// </summary>
    /// <param name="outcome">Why: not <see cref="AdmissionOutcome.Admitted"/> and not <see cref="AdmissionOutcome.Parked"/>.</param>
    /// <returns>The decision.</returns>
    public static AdmissionDecision Refused(AdmissionOutcome outcome) => new(outcome, null, null, null);

    /// <summary>
    /// Creates the decision to make the work wait for the next period.
    /// </summary>
    /// <param name="until">When the period renews.</param>
    /// <param name="because">Which limit parked the work.</param>
    /// <returns>The decision.</returns>
    public static AdmissionDecision Parked(DateTimeOffset until, AdmissionOutcome because) => new(AdmissionOutcome.Parked, null, until, because);
}
