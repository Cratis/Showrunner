// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.StageOperator.Building;
using Cratis.Showrunner.StageOperator.Resources;

namespace Cratis.Showrunner.StageOperator.Reconciling;

/// <summary>Represents everything one reconcile decided to do.</summary>
/// <param name="Status">The status to record.</param>
/// <param name="EnsureFinalizer">Whether the retention finalizer must be added.</param>
/// <param name="Apply">The objects to apply, or null when none.</param>
/// <param name="StopWorkloads">Whether the Deployment and MongoDB pods are scaled to zero.</param>
/// <param name="DeleteIngress">Whether the Ingress is removed.</param>
/// <param name="DeleteData">Whether the MongoDB StatefulSet and volume are removed.</param>
/// <param name="RemoveFinalizer">Whether the retention finalizer is released.</param>
/// <param name="RequeueAfter">When to look at the resource again before the next resync.</param>
public record ReconcilePlan(
    StageApplicationStatus Status,
    bool EnsureFinalizer = false,
    DesiredObjects? Apply = null,
    bool StopWorkloads = false,
    bool DeleteIngress = false,
    bool DeleteData = false,
    bool RemoveFinalizer = false,
    TimeSpan? RequeueAfter = null);
