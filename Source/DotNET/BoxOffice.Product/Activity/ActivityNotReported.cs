// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.EventSequences;

namespace Cratis.Showrunner.BoxOffice.Product.Activity;

/// <summary>
/// The exception that is thrown when the outbox refused to take a report of activity.
/// </summary>
/// <param name="tenant">The tenant the activity was in.</param>
/// <param name="result">The <see cref="AppendResult"/> that said why.</param>
public class ActivityNotReported(TenantId tenant, AppendResult result)
    : Exception($"The outbox refused the activity in tenant '{tenant.Value}' with {result.Errors.Count()} error(s), {result.ConstraintViolations.Count()} constraint violation(s) and {result.IdempotencyConflicts.Count()} idempotency conflict(s).");
