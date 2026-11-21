// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Link.Protocol;

/// <summary>
/// An instance asks Direct to govern a remedy while connected.
/// </summary>
/// <param name="Request">An identity the instance chose - asking again with the same one is idempotent.</param>
/// <param name="Remedy">The remedy's name.</param>
/// <param name="ConfigurationRevision">The revision the instance runs.</param>
/// <param name="Evidence">The outbox event of the failing observation the remedy answers.</param>
/// <param name="EvidenceObservedAt">When that observation was made.</param>
public record WatchdogRemedyRequest(Guid Request, string Remedy, long ConfigurationRevision, Guid Evidence, DateTimeOffset EvidenceObservedAt);
