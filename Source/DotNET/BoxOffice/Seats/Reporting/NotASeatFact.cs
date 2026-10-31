// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Seats.Reporting;

/// <summary>
/// The exception that is thrown when something that is not a seat fact is mapped to what the operator is told.
/// </summary>
/// <param name="factType">The type of the fact.</param>
public class NotASeatFact(Type factType) : Exception($"'{factType.Name}' is not a seat fact, so the operator is not told about it.");
