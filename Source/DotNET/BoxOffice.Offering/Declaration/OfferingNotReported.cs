// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.EventSequences;

namespace Cratis.Showrunner.BoxOffice.Offering.Declaration;

/// <summary>
/// Thrown when the event store refuses the product's offering declaration.
/// </summary>
/// <param name="product">The product.</param>
/// <param name="result">What the event store answered.</param>
public class OfferingNotReported(ProductId product, AppendResult result)
    : Exception($"The offering of product '{product}' could not be reported: {result.Errors.Count()} error(s) and {result.ConstraintViolations.Count()} constraint violation(s).");
