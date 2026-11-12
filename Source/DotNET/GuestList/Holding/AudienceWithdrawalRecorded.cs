// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.GuestList.Holding;

/// <summary>
/// Recorded when the product received the operator's withdrawal of an audience. It is appended to the stream of the level
/// the audience is withdrawn from, so it removes the grant of that level only.
/// </summary>
/// <param name="Level">Whom the audience is withdrawn from.</param>
/// <param name="Audience">The audience withdrawn.</param>
[EventType]
public record AudienceWithdrawalRecorded(AudienceLevel Level, AudienceName Audience);
