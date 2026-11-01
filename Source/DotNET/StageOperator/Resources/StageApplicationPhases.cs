// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageOperator.Resources;

/// <summary>The phases an application goes through.</summary>
public static class StageApplicationPhases
{
    /// <summary>Waiting for the first rollout to start.</summary>
    public const string Pending = "Pending";

    /// <summary>Rolling out a version.</summary>
    public const string Deploying = "Deploying";

    /// <summary>Running.</summary>
    public const string Running = "Running";

    /// <summary>Rolling back to the previous image.</summary>
    public const string RollingBack = "RollingBack";

    /// <summary>The resource is refused or the first rollout failed.</summary>
    public const string Failed = "Failed";

    /// <summary>The lifetime has ended.</summary>
    public const string Expired = "Expired";

    /// <summary>The resource is being deleted.</summary>
    public const string Stopped = "Stopped";
}
