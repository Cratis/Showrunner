// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;

namespace Cratis.Showrunner.BoxOffice.Compute;

/// <summary>
/// The exception that is thrown when the event log refuses a fact about the tenant's compute.
/// </summary>
/// <param name="factType">The type of the fact.</param>
/// <param name="eventSourceId">The stream the fact was appended to.</param>
/// <param name="namespace">The tenant's namespace.</param>
/// <param name="result">The result of the append.</param>
public class ComputeFactNotRecorded(Type factType, EventSourceId eventSourceId, EventStoreNamespaceName @namespace, AppendResult result)
    : Exception($"The event log in namespace '{@namespace}' refused '{factType.Name}' for '{eventSourceId}' with {result.Errors.Count()} error(s), {result.ConstraintViolations.Count()} constraint violation(s) and {result.IdempotencyConflicts.Count()} idempotency conflict(s).");
