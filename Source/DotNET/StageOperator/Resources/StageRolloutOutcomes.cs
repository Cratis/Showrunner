// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageOperator.Resources;

/// <summary>The outcomes of a rollout.</summary>
public static class StageRolloutOutcomes
{
    /// <summary>The version is running.</summary>
    public const string RolledOut = "RolledOut";

    /// <summary>The version did not become ready and the previous image was restored.</summary>
    public const string RolledBack = "RolledBack";

    /// <summary>The version did not become ready and there was nothing to roll back to.</summary>
    public const string Failed = "Failed";
}
