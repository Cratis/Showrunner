// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// The managed graph of a Production: Neo4j Community in a StatefulSet with a persistent volume, and the Secret holding its address and generated password.
/// </summary>
public static class ManagedGraph
{
    /// <summary>
    /// The key of the Secret Neo4j reads its initial credentials from, as <c>neo4j/&lt;password&gt;</c>.
    /// </summary>
    public const string AuthKey = "NEO4J_AUTH";

    const string Component = "neo4j";
    const int Bolt = 7687;
    const int Http = 7474;
    const long Neo4jUser = 7474;

    /// <summary>
    /// Creates the Secret of the graph. The password is generated once and kept: pass the one already stored.
    /// </summary>
    /// <param name="production">The Production.</param>
    /// <param name="password">The password.</param>
    /// <returns>The Secret.</returns>
    public static V1Secret Secret(Production production, string password)
    {
        var names = ProductionNames.For(production);
        return ProductionMetadata.Secret(production, names.Neo4j, Component, new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [ProductionGraph.ServerKey] = names.Neo4jServer,
            [ProductionGraph.PasswordKey] = password,
            [AuthKey] = $"{ProductionGraph.ManagedUserName}/{password}"
        });
    }

    /// <summary>
    /// Creates the Service of the graph, reachable only inside the cluster.
    /// </summary>
    /// <param name="production">The Production.</param>
    /// <returns>The Service.</returns>
    public static V1Service Service(Production production)
    {
        var names = ProductionNames.For(production);
        return new()
        {
            ApiVersion = "v1",
            Kind = "Service",
            Metadata = ProductionMetadata.Create(production, names.Neo4j, Component),
            Spec = new()
            {
                Type = "ClusterIP",
                Selector = ProductionMetadata.Labels(production, Component),
                Ports =
                [
                    new() { Name = "bolt", Port = Bolt, TargetPort = "bolt" },
                    new() { Name = "http", Port = Http, TargetPort = "http" }
                ]
            }
        };
    }

    /// <summary>
    /// Creates the StatefulSet of the graph.
    /// </summary>
    /// <param name="production">The Production, with a Managed graph.</param>
    /// <returns>The StatefulSet.</returns>
    public static V1StatefulSet StatefulSet(Production production)
    {
        var names = ProductionNames.For(production);
        var graph = production.Spec.Graph;
        var labels = ProductionMetadata.Labels(production, Component);

        var claim = new V1PersistentVolumeClaim
        {
            Metadata = new() { Name = "data" },
            Spec = new()
            {
                AccessModes = ["ReadWriteOnce"],
                StorageClassName = string.IsNullOrWhiteSpace(graph.StorageClassName) ? null : graph.StorageClassName,
                Resources = new() { Requests = new Dictionary<string, ResourceQuantity> { ["storage"] = new(graph.StorageSize) } }
            }
        };

        return new()
        {
            ApiVersion = "apps/v1",
            Kind = "StatefulSet",
            Metadata = ProductionMetadata.Create(production, names.Neo4j, Component),
            Spec = new()
            {
                Replicas = 1,
                ServiceName = names.Neo4j,
                Selector = new() { MatchLabels = labels },
                VolumeClaimTemplates = [claim],
                Template = new()
                {
                    Metadata = new() { Labels = labels },
                    Spec = new()
                    {
                        AutomountServiceAccountToken = false,
                        NodeSelector = graph.NodeSelector.Count > 0 ? new Dictionary<string, string>(graph.NodeSelector, StringComparer.Ordinal) : null,
                        SecurityContext = new() { RunAsNonRoot = true, RunAsUser = Neo4jUser, RunAsGroup = Neo4jUser, FsGroup = Neo4jUser, SeccompProfile = new() { Type = "RuntimeDefault" } },
                        Containers =
                        [
                            new()
                            {
                                Name = Component,
                                Image = graph.ImageOrDefault(),
                                Ports =
                                [
                                    new() { Name = "bolt", ContainerPort = Bolt },
                                    new() { Name = "http", ContainerPort = Http }
                                ],
                                Env =
                                [
                                    new() { Name = AuthKey, ValueFrom = new() { SecretKeyRef = new() { Name = names.Neo4j, Key = AuthKey } } }
                                ],
                                VolumeMounts = [new() { Name = "data", MountPath = "/data" }],
                                ReadinessProbe = new() { TcpSocket = new() { Port = "bolt" }, PeriodSeconds = 5 },
                                SecurityContext = new() { AllowPrivilegeEscalation = false, Capabilities = new() { Drop = ["ALL"] } }
                            }
                        ]
                    }
                }
            }
        };
    }
}
