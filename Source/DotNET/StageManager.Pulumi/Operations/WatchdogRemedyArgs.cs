// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Operations;

/// <summary>
/// What a remedy defined in Direct does in the cluster: a rolling restart of one named Deployment. Direct keeps governing whether it
/// runs; the watchdog gets get and patch on exactly this Deployment and nothing else.
/// </summary>
public sealed class WatchdogRemedyArgs
{
    /// <summary>
    /// Gets the name of the remedy in Direct.
    /// </summary>
    public required string Remedy { get; init; }

    /// <summary>
    /// Gets the namespace of the Deployment.
    /// </summary>
    public required string Namespace { get; init; }

    /// <summary>
    /// Gets the name of the Deployment to restart.
    /// </summary>
    public required string Deployment { get; init; }
}
