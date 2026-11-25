// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.GuestList.Holding;

/// <summary>
/// The newest decision the operator took about one part at one level.
/// </summary>
/// <param name="Part">The part.</param>
/// <param name="IsSuspended">Whether the part is suspended; <see langword="false"/> when it was resumed.</param>
/// <param name="Decided">When the operator decided.</param>
public record PartDecision(PartPath Part, bool IsSuspended, DateTimeOffset Decided);
