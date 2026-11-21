// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Link.Protocol;

/// <summary>
/// Why an offline remedy decision came out the way it did.
/// </summary>
public enum OfflineRemedyReason
{
    /// <summary>Every condition holds; the exact remedy may run once.</summary>
    Permitted = 0,

    /// <summary>There is no configuration, or its signature does not verify.</summary>
    UnsignedConfiguration = 1,

    /// <summary>The configuration was issued for another instance.</summary>
    WrongInstance = 2,

    /// <summary>The configuration expired.</summary>
    ExpiredConfiguration = 3,

    /// <summary>The configuration names no such remedy.</summary>
    UnknownRemedy = 4,

    /// <summary>The configuration permits no remedy offline.</summary>
    OfflineRemediesDisabled = 5,

    /// <summary>The remedy's offline mode is Never - observe and queue only.</summary>
    ObserveOnly = 6,

    /// <summary>The remedy's offline mode is Review - a person must approve, which cannot happen offline.</summary>
    ReviewRequired = 7,

    /// <summary>The outbox is not on durable storage, so the attempt could not be recorded and replayed.</summary>
    NoDurableQueue = 8,

    /// <summary>The instance cannot be sure it still holds the current fencing epoch.</summary>
    FencingUncertain = 9,

    /// <summary>The remedy names no verification check, or the configuration does not contain it.</summary>
    MissingVerification = 10,

    /// <summary>The state of the target is unknown.</summary>
    UnknownState = 11,

    /// <summary>The target's check is passing - nothing to remedy.</summary>
    NothingToRemedy = 12,

    /// <summary>The failing observation is older than the remedy's evidence freshness, or from the future.</summary>
    StaleEvidence = 13,

    /// <summary>The attempt budget for the window is spent.</summary>
    AttemptBudgetExhausted = 14,

    /// <summary>The previous attempt is inside the cooldown.</summary>
    CoolingDown = 15,
}
