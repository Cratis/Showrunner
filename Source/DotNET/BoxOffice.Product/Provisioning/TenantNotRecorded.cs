// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.EventSequences;

namespace Cratis.Showrunner.BoxOffice.Product.Provisioning;

/// <summary>
/// The exception that is thrown when a fact about provisioning could not be recorded. The registration is retried.
/// </summary>
/// <param name="eventType">The type of the fact.</param>
/// <param name="tenant">The tenant it concerns.</param>
/// <param name="result">The <see cref="AppendResult"/> that said why.</param>
public class TenantNotRecorded(Type eventType, TenantId tenant, AppendResult result)
    : Exception($"A fact about provisioning tenant '{tenant.Value}' was refused: '{eventType.Name}' got {result.Errors.Count()} error(s), {result.ConstraintViolations.Count()} constraint violation(s) and {result.IdempotencyConflicts.Count()} idempotency conflict(s).");
