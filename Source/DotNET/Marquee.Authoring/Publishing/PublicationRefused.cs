// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;

namespace Cratis.Showrunner.Marquee.Authoring.Publishing;

/// <summary>
/// The exception that is thrown when the outbox refuses a public event. It fails the partition, so Chronicle delivers
/// the private event again instead of the publication being lost.
/// </summary>
/// <param name="publicEventType">The type of public event that was refused.</param>
/// <param name="announcementId">The announcement it was published for.</param>
/// <param name="namespace">The namespace whose outbox refused it.</param>
/// <param name="result">The result of the append.</param>
public class PublicationRefused(Type publicEventType, EventSourceId announcementId, EventStoreNamespaceName @namespace, AppendResult result)
    : Exception($"The outbox in namespace '{@namespace}' refused '{publicEventType.Name}' for announcement '{announcementId}' with {result.Errors.Count()} error(s), {result.ConstraintViolations.Count()} constraint violation(s) and {(result.HasConcurrencyViolations ? "a" : "no")} concurrency violation.");
