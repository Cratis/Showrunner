// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.GuestList.Authoring.Decisions;

/// <summary>
/// Defines what a command that suspends or resumes a part says, so both are held to the same rules.
/// </summary>
public interface ISuspensionDecision
{
    /// <summary>
    /// Gets the product the decision is for.
    /// </summary>
    Product Product { get; }

    /// <summary>
    /// Gets whom the part is suspended or resumed for.
    /// </summary>
    SuspensionLevel Level { get; }

    /// <summary>
    /// Gets the tenant, for <see cref="SuspensionLevel.Tenant"/> and <see cref="SuspensionLevel.User"/>; empty for
    /// <see cref="SuspensionLevel.System"/>.
    /// </summary>
    TenantId TenantId { get; }

    /// <summary>
    /// Gets the user, for <see cref="SuspensionLevel.User"/>; empty for the other levels.
    /// </summary>
    UserId UserId { get; }

    /// <summary>
    /// Gets the path of the part.
    /// </summary>
    PartPath Part { get; }
}
