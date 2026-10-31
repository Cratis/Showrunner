// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.Activity;

/// <summary>
/// Published by the product once a day with how much a tenant was used. Replaces Direct's
/// RecordCustomerActivity report.
/// </summary>
/// <param name="TenantId">The tenant this concerns.</param>
/// <param name="Day">The day.</param>
/// <param name="ActiveUsers">How many people were active.</param>
/// <param name="Sessions">How many sessions there were.</param>
/// <param name="SignIns">How many sign-ins there were.</param>
[EventType]
[Public]
public record ActivitySummarized(
    TenantId TenantId,
    ActivityDay Day,
    ActiveUserCount ActiveUsers,
    SessionCount Sessions,
    SignInCount SignIns);
