// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.GuestList.Availability.Declaration;

/// <summary>
/// What reporting the declaration of a product came to.
/// </summary>
public enum AvailabilityReport
{
    /// <summary>
    /// The declaration was reported to the operator.
    /// </summary>
    Reported = 0,

    /// <summary>
    /// The operator already has this declaration, so nothing was reported.
    /// </summary>
    Unchanged = 1,

    /// <summary>
    /// The product declares nothing, so there is nothing to report.
    /// </summary>
    NothingDeclared = 2
}
