// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.EventSequences;

namespace Cratis.Showrunner.BoxOffice.Offering.Metering;

/// <summary>
/// Exception that gets thrown when the event store refuses a compute event, so the day is not marked as reported.
/// </summary>
/// <param name="tenant">The tenant the event is for.</param>
/// <param name="day">The day the event is for.</param>
/// <param name="result">The result the event store answered with.</param>
public class ComputeNotReported(TenantId tenant, ComputeDay day, AppendResult result)
    : Exception($"The event store refused to record the compute of tenant '{tenant}' for {day.Value:yyyy-MM-dd}: {string.Join("; ", result.Errors)}");
