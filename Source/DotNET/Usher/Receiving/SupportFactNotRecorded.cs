// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;

namespace Cratis.Showrunner.Usher.Receiving;

/// <summary>
/// The exception that is thrown when the event log of a tenant refuses a fact translated from the desk. It fails the
/// inbox partition, so the desk event is translated again instead of being lost.
/// </summary>
/// <param name="privateEventType">The type of private event that was refused.</param>
/// <param name="eventSourceId">The issue it was recorded for.</param>
/// <param name="namespace">The namespace whose event log refused it.</param>
/// <param name="result">The result of the append.</param>
public class SupportFactNotRecorded(Type privateEventType, EventSourceId eventSourceId, EventStoreNamespaceName @namespace, AppendResult result)
    : Exception($"The event log in namespace '{@namespace}' refused '{privateEventType.Name}' for issue '{eventSourceId}' with {result.Errors.Count()} error(s), {result.ConstraintViolations.Count()} constraint violation(s) and {result.IdempotencyConflicts.Count()} idempotency conflict(s).");
