// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// Creates the NetworkPolicy that limits what the pods of the application connect to, from <see cref="ApplicationSpec.Egress"/>.
/// </summary>
public static class ApplicationEgressPolicy
{
    /// <summary>
    /// The private address ranges a rule that opens the public internet leaves closed.
    /// </summary>
    public static readonly string[] PrivateRanges = ["10.0.0.0/8", "172.16.0.0/12", "192.168.0.0/16"];

    /// <summary>
    /// Creates the NetworkPolicy.
    /// </summary>
    /// <param name="performance">The Performance that owns it.</param>
    /// <returns>The NetworkPolicy, or null when the application names no egress rule.</returns>
    public static V1NetworkPolicy? Create(Performance performance)
    {
        var rules = performance.Spec.Application.Egress;
        if (rules.Count == 0)
        {
            return null;
        }

        var names = PerformanceNames.For(performance);
        List<V1NetworkPolicyEgressRule> egress =
        [
            new()
            {
                To = [new() { NamespaceSelector = new() { MatchLabels = new Dictionary<string, string> { ["kubernetes.io/metadata.name"] = "kube-system" } } }],
                Ports = [new() { Port = 53, Protocol = "UDP" }, new() { Port = 53, Protocol = "TCP" }]
            },
            new() { To = [new() { PodSelector = new() }] },
            .. rules.Select(Rule)
        ];

        return new()
        {
            ApiVersion = "networking.k8s.io/v1",
            Kind = "NetworkPolicy",
            Metadata = PerformanceMetadata.Create(performance, names.EgressPolicy, "application"),
            Spec = new()
            {
                PodSelector = new() { MatchLabels = PerformanceMetadata.Labels(performance, "application") },
                PolicyTypes = ["Egress"],
                Egress = egress
            }
        };
    }

    static V1NetworkPolicyEgressRule Rule(ApplicationEgressRule rule) => new()
    {
        To = [Peer(rule)],
        Ports = [.. rule.Ports.Select(port => new V1NetworkPolicyPort { Port = port, Protocol = "TCP" })]
    };

    static V1NetworkPolicyPeer Peer(ApplicationEgressRule rule)
    {
        if (!string.IsNullOrWhiteSpace(rule.Namespace))
        {
            return new() { NamespaceSelector = new() { MatchLabels = new Dictionary<string, string> { ["kubernetes.io/metadata.name"] = rule.Namespace } } };
        }

        return string.IsNullOrWhiteSpace(rule.Cidr)
            ? new() { IpBlock = new() { Cidr = "0.0.0.0/0", Except = [.. PrivateRanges] } }
            : new() { IpBlock = new() { Cidr = rule.Cidr } };
    }
}
