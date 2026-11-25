// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.GuestList.Holding;

/// <summary>
/// Recorded when the product received the operator's suspension of a part. It is the product's own fact, appended to the
/// stream of the level the part is suspended at: the tenant's, a user's or the system's. When it occurred is when the
/// operator decided, so the newest decision wins whatever order the decisions arrive in.
/// </summary>
/// <param name="Level">Whom the part is suspended for.</param>
/// <param name="Part">The path of the part suspended.</param>
[EventType]
public record PartSuspensionRecorded(SuspensionLevel Level, PartPath Part);
