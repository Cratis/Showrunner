// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.Lifecycle;

/// <summary>
/// Published by the application when the reporter withdraws an issue they no longer need help with. Nothing more
/// happens on the issue from the reporter's side afterwards.
/// </summary>
/// <param name="TenantId">The tenant the issue belongs to.</param>
[EventType]
[Public]
public record IssueWithdrawn(TenantId TenantId);
