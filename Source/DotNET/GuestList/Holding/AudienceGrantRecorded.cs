// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.GuestList.Holding;

/// <summary>
/// Recorded when the product received the operator's grant of an audience. It is the product's own fact, appended to the
/// stream of the level the audience is granted at: the tenant's, a user's or the system's. When it occurred is when the
/// operator decided, so the newest decision wins whatever order the decisions arrive in.
/// </summary>
/// <param name="Level">Whom the audience is granted to.</param>
/// <param name="Audience">The audience granted.</param>
[EventType]
public record AudienceGrantRecorded(AudienceLevel Level, AudienceName Audience);
