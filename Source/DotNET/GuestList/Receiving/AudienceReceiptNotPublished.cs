// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.EventSequences;

namespace Cratis.Showrunner.GuestList.Receiving;

/// <summary>
/// Thrown when the receipt of a recorded decision could not be published. The reactor then delivers the decision again.
/// </summary>
/// <param name="level">Whom the decision was for.</param>
/// <param name="audience">The audience decided.</param>
/// <param name="result">The result of the append.</param>
public class AudienceReceiptNotPublished(AudienceLevel level, AudienceName audience, AppendResult result)
    : Exception($"The receipt of the decision about audience '{audience}' at level '{level}' could not be published: {result.Errors.Count()} error(s) and {result.ConstraintViolations.Count()} constraint violation(s).");
