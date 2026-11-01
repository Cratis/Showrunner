// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageOperator.Reconciling;

/// <summary>The condition types of an application.</summary>
public static class ConditionTypes
{
    /// <summary>The application is running its version and every pod is ready.</summary>
    public const string Ready = "Ready";

    /// <summary>A rollout is in progress.</summary>
    public const string Progressing = "Progressing";

    /// <summary>The application is refused, failed or runs an older version than asked for.</summary>
    public const string Degraded = "Degraded";

    /// <summary>The lifetime has ended.</summary>
    public const string Expired = "Expired";
}
