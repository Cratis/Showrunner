// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Entitlements;

/// <summary>
/// Why a tenant can read but not change anything.
/// </summary>
public enum ReadOnlyReason
{
    /// <summary>
    /// The tenant can change things.
    /// </summary>
    None = 0,

    /// <summary>
    /// The operator suspended the tenant.
    /// </summary>
    TenantSuspended = 1,

    /// <summary>
    /// The trial is over and no seats were granted or bought.
    /// </summary>
    TrialClosed = 2
}
