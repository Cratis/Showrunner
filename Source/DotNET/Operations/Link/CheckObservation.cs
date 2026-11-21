// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Operations.Link.Protocol;

namespace Cratis.Showrunner.Operations.Link;

/// <summary>
/// What the link last observed of one check.
/// </summary>
/// <param name="State">Whether the check is failing or passing.</param>
/// <param name="Failures">How many passes in a row it has failed.</param>
/// <param name="FirstFailedAt">When the current failure episode began.</param>
/// <param name="ObservedAt">When it was last observed.</param>
/// <param name="StatusCode">The status code last received, zero for none.</param>
/// <param name="EvidenceEvent">The outbox event of the failing observation - the evidence a remedy cites.</param>
public sealed record CheckObservation(
    ObservedCheckState State,
    int Failures,
    DateTimeOffset FirstFailedAt,
    DateTimeOffset ObservedAt,
    int StatusCode,
    Guid EvidenceEvent);
