// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Product.Activity;

/// <summary>
/// What became of a report that a person was active.
/// </summary>
public enum ActivityOutcome
{
    /// <summary>
    /// The operator was told the person was active on the day.
    /// </summary>
    Reported = 0,

    /// <summary>
    /// The operator already knows the person was active on the day, so nothing was recorded.
    /// </summary>
    AlreadyReported = 1,

    /// <summary>
    /// The product has no such tenant.
    /// </summary>
    UnknownTenant = 2,

    /// <summary>
    /// The tenant has not been told which of the operator's organizations it is, so nothing can be reported under it yet.
    /// </summary>
    OrganizationNotKnown = 3
}
