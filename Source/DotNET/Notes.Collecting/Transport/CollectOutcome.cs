// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Collecting.Transport;

/// <summary>
/// How the collector received a batch of notes.
/// </summary>
public enum CollectOutcome
{
    /// <summary>
    /// Every note was stored or was already.
    /// </summary>
    Stored = 0,

    /// <summary>
    /// A note names another tenant than its publication does; nothing was stored.
    /// </summary>
    TenantMismatch = 1,

    /// <summary>
    /// The publication's tenant has no single namespace in the collecting store; nothing was stored.
    /// </summary>
    UnknownTenant = 2,

    /// <summary>
    /// A note could not be read; nothing was stored.
    /// </summary>
    Invalid = 3
}
