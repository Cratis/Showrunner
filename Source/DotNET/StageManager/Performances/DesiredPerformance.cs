// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// Everything a Performance should have in the cluster, ready to apply.
/// </summary>
/// <param name="Secrets">The Secrets, which must exist before the workloads that mount them.</param>
/// <param name="Deployments">The Deployments.</param>
/// <param name="Services">The Services.</param>
/// <param name="Ingress">The Ingress, or null when the Performance has none.</param>
/// <param name="NetworkPolicy">The NetworkPolicy of a published application, or null.</param>
public record DesiredPerformance(
    IReadOnlyList<V1Secret> Secrets,
    IReadOnlyList<V1Deployment> Deployments,
    IReadOnlyList<V1Service> Services,
    V1Ingress? Ingress,
    V1NetworkPolicy? NetworkPolicy = null);
