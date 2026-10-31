// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.Trials;

/// <summary>
/// Published by Direct when a tenant's trial starts. The trial seats count while the trial runs. Replaces
/// Direct's CustomerTrialStarted.
/// </summary>
/// <param name="TenantId">The tenant this concerns.</param>
/// <param name="Ends">The last day of the trial.</param>
/// <param name="Seats">The seats the trial includes.</param>
[EventType]
[Public]
public record TrialOpened(TenantId TenantId, TrialEnd Ends, SeatCount Seats);
