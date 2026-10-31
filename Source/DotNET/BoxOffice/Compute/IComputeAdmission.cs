// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Compute;

/// <summary>
/// Decides, before a unit of work starts, whether the current tenant may run it.
/// </summary>
/// <remarks>
/// Usage is what the metering has measured so far, so it trails the work by the metering's poll interval, and work that is
/// running has not been measured yet. The concurrency limit is exact: it is decided from the tenant's own start and stop
/// facts.
/// </remarks>
public interface IComputeAdmission
{
    /// <summary>
    /// Decides whether the tenant may start a unit of work of a meter.
    /// </summary>
    /// <param name="meter">The meter the work will use.</param>
    /// <returns>The decision. Start the work only when it is admitted, and end the <see cref="AdmissionDecision.Work"/> when the work stops.</returns>
    /// <exception cref="MeterNotDeclared">The product did not declare the meter.</exception>
    /// <exception cref="ComputeFactNotRecorded">The event log refused to record the work or the allowance running out.</exception>
    Task<AdmissionDecision> Admit(MeterId meter);

    /// <summary>
    /// Ends a unit of work that was admitted, which frees its concurrent slot. Ending it twice, or after its lease ended,
    /// changes nothing.
    /// </summary>
    /// <param name="work">The <see cref="AdmissionDecision.Work"/> of the decision.</param>
    /// <returns>An awaitable task.</returns>
    /// <exception cref="ComputeFactNotRecorded">The event log refused to record the end.</exception>
    Task End(ComputeWork work);
}
