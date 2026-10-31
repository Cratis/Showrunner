// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.Tenants;

/// <summary>
/// Why a tenant is suspended and the product is read-only for it.
/// </summary>
public enum SuspensionCause
{
    /// <summary>
    /// The trial ended and no seats were granted or bought.
    /// </summary>
    TrialEndedWithoutSeats = 0,

    /// <summary>
    /// A payment is overdue.
    /// </summary>
    PaymentOverdue = 1,

    /// <summary>
    /// An administrator suspended the tenant.
    /// </summary>
    AdministratorDecision = 2
}
