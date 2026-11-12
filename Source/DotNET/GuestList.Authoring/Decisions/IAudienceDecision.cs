// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.GuestList.Authoring.Decisions;

/// <summary>
/// Defines what a command that grants or withdraws an audience says, so both are held to the same rules.
/// </summary>
public interface IAudienceDecision
{
    /// <summary>
    /// Gets the product the decision is for.
    /// </summary>
    Product Product { get; }

    /// <summary>
    /// Gets whom the audience is granted to or withdrawn from.
    /// </summary>
    AudienceLevel Level { get; }

    /// <summary>
    /// Gets the tenant, for <see cref="AudienceLevel.Tenant"/> and <see cref="AudienceLevel.User"/>; empty for
    /// <see cref="AudienceLevel.System"/>.
    /// </summary>
    TenantId TenantId { get; }

    /// <summary>
    /// Gets the user, for <see cref="AudienceLevel.User"/>; empty for the other levels.
    /// </summary>
    UserId UserId { get; }

    /// <summary>
    /// Gets the audience.
    /// </summary>
    AudienceName Audience { get; }
}
