// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Contracts.Trials;

/// <summary>
/// Published by Direct when a trial is over. Replaces Direct's CustomerTrialEnded.
/// </summary>
/// <param name="TenantId">The tenant this concerns.</param>
/// <param name="Closure">How the trial ended.</param>
[EventType]
[Public]
public record TrialClosed(TenantId TenantId, TrialClosure Closure);
