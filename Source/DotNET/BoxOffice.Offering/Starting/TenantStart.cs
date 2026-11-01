// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Offering.Starting;

/// <summary>
/// What starting a tenant on the default offering did.
/// </summary>
public enum TenantStart
{
    /// <summary>
    /// The tenant started on the default offering now, and the operator was told.
    /// </summary>
    Started = 0,

    /// <summary>
    /// The tenant had started on the default offering already, so nothing changed.
    /// </summary>
    AlreadyStarted = 1
}
