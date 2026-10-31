// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;
using Cratis.Chronicle.EventSequences;

namespace Cratis.Showrunner.Marquee.Receiving;

/// <summary>
/// The exception that is thrown when the event log refuses what the product received. It fails the partition, so
/// Chronicle delivers the event again instead of the announcement being lost.
/// </summary>
/// <param name="eventType">The type of event that was refused.</param>
/// <param name="announcementId">The announcement it is about.</param>
/// <param name="authoringStore">The authoring store it came from.</param>
/// <param name="result">The result of the append.</param>
public class AnnouncementNotRecorded(Type eventType, EventSourceId announcementId, EventStoreName authoringStore, AppendResult result)
    : Exception($"The event log refused '{eventType.Name}' for announcement '{announcementId}' from authoring store '{authoringStore}' with {result.Errors.Count()} error(s), {result.ConstraintViolations.Count()} constraint violation(s) and {(result.HasConcurrencyViolations ? "a" : "no")} concurrency violation.");
