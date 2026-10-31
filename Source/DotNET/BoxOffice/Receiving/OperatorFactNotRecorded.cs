// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;

namespace Cratis.Showrunner.BoxOffice.Receiving;

/// <summary>
/// The exception that is thrown when the event log refuses a fact translated from an operator's decision. The decision is
/// not lost: the inbox delivers it again.
/// </summary>
/// <param name="factType">The type of the private fact.</param>
/// <param name="eventSourceId">The event source the fact was for.</param>
/// <param name="namespace">The namespace of the event log that refused it.</param>
/// <param name="result">The refused <see cref="AppendResult"/>.</param>
public class OperatorFactNotRecorded(Type factType, EventSourceId eventSourceId, EventStoreNamespaceName @namespace, AppendResult result)
    : Exception($"The event log in namespace '{@namespace}' refused '{factType.Name}' for '{eventSourceId}' with {result.Errors.Count()} error(s), {result.ConstraintViolations.Count()} constraint violation(s) and {result.IdempotencyConflicts.Count()} idempotency conflict(s).");
