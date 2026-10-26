// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.Conversation;

/// <summary>
/// Published by the application when the reporter adds something to an issue of their own accord - including what
/// they wrote alongside an error report.
/// </summary>
/// <param name="TenantId">The tenant the issue belongs to.</param>
/// <param name="Details">What the reporter added.</param>
[EventType]
[Public]
public record DetailsAdded(TenantId TenantId, Details Details);
