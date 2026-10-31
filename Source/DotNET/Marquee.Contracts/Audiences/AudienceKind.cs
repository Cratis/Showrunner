// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Marquee.Contracts.Audiences;

/// <summary>
/// Which kind of audience an <see cref="Audience"/> is. It decides which of the audience's other values apply.
/// </summary>
public enum AudienceKind
{
    /// <summary>
    /// Everyone using the product.
    /// </summary>
    Everyone = 0,

    /// <summary>
    /// The tenants named in <see cref="Audience.TenantIds"/>.
    /// </summary>
    Tenants = 1,

    /// <summary>
    /// The tenants that hold the module in <see cref="Audience.Module"/>.
    /// </summary>
    HoldersOfModule = 2,

    /// <summary>
    /// The tenants on the plan in <see cref="Audience.Plan"/>.
    /// </summary>
    OnPlan = 3
}
