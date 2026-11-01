// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// Creates the NetworkPolicy of a published Stage application: only AuthProxy of the same Performance may connect to it. Stage believes the principal headers of the addresses it trusts, so nothing else may reach it from inside the cluster either.
/// </summary>
public static class PublishedNetworkPolicy
{
    /// <summary>
    /// Creates the NetworkPolicy.
    /// </summary>
    /// <param name="performance">The Performance that owns it.</param>
    /// <returns>The NetworkPolicy.</returns>
    public static V1NetworkPolicy Create(Performance performance)
    {
        var names = PerformanceNames.For(performance);
        return new()
        {
            ApiVersion = "networking.k8s.io/v1",
            Kind = "NetworkPolicy",
            Metadata = PerformanceMetadata.Create(performance, names.NetworkPolicy, "application"),
            Spec = new()
            {
                PodSelector = new() { MatchLabels = PerformanceMetadata.Labels(performance, "application") },
                PolicyTypes = ["Ingress"],
                Ingress =
                [
                    new()
                    {
                        FromProperty = [new() { PodSelector = new() { MatchLabels = PerformanceMetadata.Labels(performance, "authproxy") } }],
                        Ports = [new() { Port = PublishedStageContract.Port, Protocol = "TCP" }]
                    }
                ]
            }
        };
    }
}
