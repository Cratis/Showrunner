// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.GuestList.Holding;

/// <summary>
/// The newest decision the operator took about one audience at one level.
/// </summary>
/// <param name="Audience">The audience.</param>
/// <param name="Granted">Whether the audience is granted; <see langword="false"/> when it was withdrawn.</param>
/// <param name="Decided">When the operator decided.</param>
public record AudienceDecision(AudienceName Audience, bool Granted, DateTimeOffset Decided);
