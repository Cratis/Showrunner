// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Product.Activity;

/// <summary>
/// Tells the operator that a person was active in a tenant, so it can show when they were last seen. AuthProxy's sign-in
/// callback reports activity by itself; a product uses this for activity AuthProxy cannot see, such as a person acting
/// through a token or a job.
/// </summary>
/// <remarks>
/// The operator is told at most once per person and day, however often this is called: seeing someone again on the same
/// day is not a fact.
/// </remarks>
public interface IReportActivity
{
    /// <summary>
    /// Reports that a person was active in a tenant today.
    /// </summary>
    /// <param name="tenant">The tenant the person was active in.</param>
    /// <param name="person">The person who was active.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the report.</param>
    /// <returns>What became of the report.</returns>
    Task<ActivityOutcome> Report(TenantId tenant, Person person, CancellationToken cancellationToken = default);
}
