// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.Lifecycle;

/// <summary>
/// Published by the desk when support resolves an issue. A fix is often resolved when it is merged and released
/// later, so the release that ships it is its own fact, <see cref="WorkItems.IssueFixedInRelease"/>.
/// </summary>
/// <param name="TenantId">The tenant the issue belongs to.</param>
/// <param name="Resolution">Support's note on how it was resolved.</param>
[EventType]
[Public]
public record IssueResolved(TenantId TenantId, Resolution Resolution);
