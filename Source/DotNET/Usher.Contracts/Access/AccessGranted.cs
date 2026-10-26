// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.Access;

/// <summary>
/// Published by the application when the reporter grants support access to sign in as them on this issue until a
/// fixed time. Granting access always shares identity, so <see cref="Identity.IssueAttributedToReporter"/> has been
/// published for the issue before this event. The grant ends with <see cref="AccessRevoked"/> or
/// <see cref="AccessExpired"/>.
/// </summary>
/// <param name="TenantId">The tenant the issue belongs to.</param>
/// <param name="ValidUntil">When the grant ends unless revoked first.</param>
[EventType]
[Public]
public record AccessGranted(TenantId TenantId, ValidUntil ValidUntil);
