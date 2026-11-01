// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.EventSequences;

namespace Cratis.Showrunner.BoxOffice.Offering.Starting;

/// <summary>
/// Thrown when the event store refuses the fact that a tenant started on the default offering.
/// </summary>
/// <param name="tenant">The tenant.</param>
/// <param name="result">What the event store answered.</param>
public class TenantNotStarted(TenantId tenant, AppendResult result)
    : Exception($"Tenant '{tenant}' could not be started on the default offering: {result.Errors.Count()} error(s) and {result.ConstraintViolations.Count()} constraint violation(s).");
