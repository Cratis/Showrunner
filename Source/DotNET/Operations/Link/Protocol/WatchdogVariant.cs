// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Link.Protocol;

/// <summary>
/// The image a watchdog instance runs - the same runtime, hosted differently.
/// </summary>
public enum WatchdogVariant
{
    /// <summary>
    /// The runtime with its own Chronicle kernel and SQLite storage.
    /// </summary>
    Full = 0,

    /// <summary>
    /// The runtime connecting to an external kernel, with a small durable local queue. The Showrunner watchdog identifies
    /// as this variant: it has no kernel of its own and keeps a bounded durable outbox.
    /// </summary>
    Slim = 1,
}
