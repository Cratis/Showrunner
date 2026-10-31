// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;

namespace Cratis.Showrunner.BoxOffice.Compute.Reporting;

/// <summary>
/// The exception that is thrown when the outbox refuses to record that a tenant's allowance ran out.
/// </summary>
/// <param name="organization">The organization of the tenant.</param>
/// <param name="namespace">The tenant's namespace.</param>
/// <param name="result">The result of the append.</param>
public class AllowanceNotReported(OrganizationId organization, EventStoreNamespaceName @namespace, AppendResult result)
    : Exception($"The outbox in namespace '{@namespace}' refused '{nameof(AllowanceExhausted)}' for organization '{organization.Value}' with {result.Errors.Count()} error(s), {result.ConstraintViolations.Count()} constraint violation(s) and {result.IdempotencyConflicts.Count()} idempotency conflict(s).");
