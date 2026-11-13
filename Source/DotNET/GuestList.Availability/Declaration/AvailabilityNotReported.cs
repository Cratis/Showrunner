// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.EventSequences;
using Cratis.Showrunner.GuestList.Contracts.Audiences;

namespace Cratis.Showrunner.GuestList.Availability.Declaration;

/// <summary>
/// Thrown when the declaration of a product could not be reported. The reporting service tries again.
/// </summary>
/// <param name="product">The product.</param>
/// <param name="result">The result of the append.</param>
public class AvailabilityNotReported(Product product, AppendResult result)
    : Exception($"The availability declared by product '{product}' could not be reported: {result.Errors.Count()} error(s) and {result.ConstraintViolations.Count()} constraint violation(s).");
