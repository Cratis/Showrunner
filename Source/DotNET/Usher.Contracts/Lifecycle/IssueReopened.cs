// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.Lifecycle;

/// <summary>
/// Published by the application when the reporter reopens an issue support had resolved, because it is not
/// solved for them.
/// </summary>
/// <param name="TenantId">The tenant the issue belongs to.</param>
/// <param name="Reason">Why the reporter reopened it.</param>
[EventType]
[Public]
public record IssueReopened(TenantId TenantId, ReopenReason Reason);
