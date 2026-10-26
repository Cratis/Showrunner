// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.Access;

/// <summary>
/// Published by the application when the live access grant on the issue reached its <see cref="ValidUntil"/>
/// without being revoked.
/// </summary>
/// <param name="TenantId">The tenant the issue belongs to.</param>
[EventType]
[Public]
public record AccessExpired(TenantId TenantId);
