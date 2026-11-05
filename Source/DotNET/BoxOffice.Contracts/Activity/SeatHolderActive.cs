// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.Activity;

/// <summary>
/// Published by the product when a person who signed in to a tenant was active on a day, at most once per person and
/// day. Direct projects it as when the person was last seen. It is a fact about the person, so it is published when
/// the day changes for them, not for every sign-in or return.
/// </summary>
/// <param name="TenantId">The tenant this concerns.</param>
/// <param name="Holder">The person who was active.</param>
/// <param name="Day">The day, in UTC, the person was active.</param>
[EventType]
[Public]
public record SeatHolderActive(TenantId TenantId, Person Holder, ActivityDay Day);
