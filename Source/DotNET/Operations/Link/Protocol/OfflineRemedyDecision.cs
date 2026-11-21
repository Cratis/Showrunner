// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Link.Protocol;

/// <summary>
/// The offline decision, in the assurance outcome vocabulary.
/// </summary>
/// <param name="Outcome"><see cref="AssuranceOutcome.Allow"/>, <see cref="AssuranceOutcome.Block"/> or
/// <see cref="AssuranceOutcome.RequiresHumanReview"/>.</param>
/// <param name="Reason">Why.</param>
/// <param name="Explanation">Why, in words.</param>
public record OfflineRemedyDecision(AssuranceOutcome Outcome, OfflineRemedyReason Reason, string Explanation)
{
    /// <summary>
    /// Gets whether the remedy may run.
    /// </summary>
    public bool IsPermitted => Outcome == AssuranceOutcome.Allow;
}
