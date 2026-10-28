// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Contracts.Identity;

/// <summary>
/// Published by the application when the reporter shares who they are, either on its own or because they granted
/// support access, which always shares identity first. Until this event the desk knows the issue but not the
/// person. The compliance subject is the issue (its event source), which belongs to exactly one reporter.
/// </summary>
/// <param name="TenantId">The tenant the issue belongs to.</param>
/// <param name="Reporter">Who reported the issue.</param>
[EventType]
[Public]
public record IssueAttributedToReporter(TenantId TenantId, Reporter Reporter);
