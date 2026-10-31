// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;

namespace Cratis.Showrunner.BoxOffice.Seats.Reporting;

/// <summary>
/// The exception that is thrown when the outbox refuses a seat report. The report is retried.
/// </summary>
/// <param name="eventType">The type of the public event.</param>
/// <param name="organization">The operator's organization it was for.</param>
/// <param name="namespace">The namespace of the outbox that refused it.</param>
/// <param name="result">The refused <see cref="AppendResult"/>.</param>
public class SeatNotReported(Type eventType, OrganizationId organization, EventStoreNamespaceName @namespace, AppendResult result)
    : Exception($"The outbox in namespace '{@namespace}' refused '{eventType.Name}' for organization '{organization.Value}' with {result.Errors.Count()} error(s), {result.ConstraintViolations.Count()} constraint violation(s) and {result.IdempotencyConflicts.Count()} idempotency conflict(s).");
