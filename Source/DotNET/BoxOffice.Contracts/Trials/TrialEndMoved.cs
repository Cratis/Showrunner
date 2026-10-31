// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.Trials;

/// <summary>
/// Published by Direct when a trial is extended or shortened. Replaces Direct's CustomerTrialEndMoved and the
/// v1 TrialEndChangedForTenant.
/// </summary>
/// <param name="TenantId">The tenant this concerns.</param>
/// <param name="Ends">The new last day of the trial.</param>
[EventType]
[Public]
public record TrialEndMoved(TenantId TenantId, TrialEnd Ends);
