// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;

namespace Cratis.Showrunner.GuestList.Receiving;

/// <summary>
/// The exception that is thrown when the event log refuses what the product received. It fails the partition, so Chronicle
/// delivers the event again instead of the decision being lost.
/// </summary>
/// <param name="eventType">The type of event that was refused.</param>
/// <param name="eventSourceId">The stream it was recorded in.</param>
/// <param name="namespace">The namespace whose event log refused it.</param>
/// <param name="operatorStore">The operator store it came from.</param>
/// <param name="result">The result of the append.</param>
public class AudienceNotRecorded(Type eventType, EventSourceId eventSourceId, EventStoreNamespaceName @namespace, EventStoreName operatorStore, AppendResult result)
    : Exception($"The event log in namespace '{@namespace}' refused '{eventType.Name}' for '{eventSourceId}' from operator store '{operatorStore}' with {result.Errors.Count()} error(s), {result.ConstraintViolations.Count()} constraint violation(s) and {(result.HasConcurrencyViolations ? "a" : "no")} concurrency violation.");
