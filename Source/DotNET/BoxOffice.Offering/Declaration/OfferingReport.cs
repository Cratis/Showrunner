// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Offering.Declaration;

/// <summary>
/// What reporting the offering amounted to.
/// </summary>
public enum OfferingReport
{
    /// <summary>
    /// The declaration differed from the last one reported, or none had been reported, and was reported now.
    /// </summary>
    Reported = 0,

    /// <summary>
    /// The last reported declaration is the same, so nothing was reported.
    /// </summary>
    Unchanged = 1
}
