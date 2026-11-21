// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Operations;

/// <summary>
/// A public site the <see cref="Watchdog"/> probe calls the way an anonymous visitor does.
/// </summary>
public sealed class WatchdogProbeTargetArgs
{
    /// <summary>
    /// Gets the name of the target.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the base address, without a trailing path.
    /// </summary>
    public required string BaseUrl { get; init; }

    /// <summary>
    /// Gets the requests to make.
    /// </summary>
    public required IList<WatchdogProbeCheckArgs> Checks { get; init; }
}
