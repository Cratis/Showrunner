// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.Access;

/// <summary>
/// Published by the desk when support asks the reporter for access to sign in as them on this issue. The reporter
/// answers by granting access, which comes back as <see cref="AccessGranted"/>, or by not acting on it.
/// </summary>
/// <param name="TenantId">The tenant the issue belongs to.</param>
/// <param name="Reason">Why support needs access.</param>
[EventType]
[Public]
public record AccessRequested(TenantId TenantId, AccessRequestReason Reason);
