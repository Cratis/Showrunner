// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// Exposes MongoDB's own health to Prometheus: one <c>mongodb_exporter</c> per member of a managed database.
/// </summary>
/// <remarks>
/// <para>
/// The numbers that matter are per member, so each exporter connects directly to its own member; a replica set connection would answer from whichever member the driver picked.
/// They are Deployments of their own, not sidecars, so adding them never rolls a mongod. Only the diagnostic data and replica set status collectors run: Chronicle creates a database
/// per event store and namespace, and series that scale with them would grow the Prometheus head without bound.
/// </para>
/// <para>
/// The credentials are the operator-managed <c>clusterMonitor</c> user, read from the users Secret into the container environment, so the password never appears in the manifest.
/// </para>
/// </remarks>
public static class MongoDBExporter
{
    /// <summary>
    /// The key of the monitoring user in the users Secret.
    /// </summary>
    public const string UserKey = "MONGODB_CLUSTER_MONITOR_USER";

    /// <summary>
    /// The key of the monitoring password in the users Secret.
    /// </summary>
    public const string PasswordKey = "MONGODB_CLUSTER_MONITOR_PASSWORD";

    /// <summary>
    /// The label that tells the exporters of one member from those of another.
    /// </summary>
    public const string MemberLabel = "showrunner.cratis.io/member";

    const string Component = "mongodb-exporter";

    /// <summary>
    /// Gets the name of the exporter of a member.
    /// </summary>
    /// <param name="names">The names of the Production.</param>
    /// <param name="member">The ordinal of the member.</param>
    /// <returns>The name of the Deployment.</returns>
    public static string NameOf(ProductionNames names, int member) => $"{names.MongoDB}-exporter-{member}";

    /// <summary>
    /// Gets the arguments that keep the exporter to the cheap collectors.
    /// </summary>
    /// <returns>The container arguments.</returns>
    public static string[] Arguments() =>
    [
        $"--web.listen-address=:{ProductionDatabaseMetrics.Port}",
        "--mongodb.direct-connect",
        "--collector.diagnosticdata",
        "--collector.replicasetstatus",
        "--no-collector.exporter-metrics",
        "--no-compatible-mode",
        "--no-discovering-mode",
        "--log.level=warn"
    ];

    /// <summary>
    /// Creates the exporter Deployments of a Production, one per member.
    /// </summary>
    /// <param name="production">The Production.</param>
    /// <returns>The Deployments.</returns>
    public static IReadOnlyList<V1Deployment> Create(Production production)
    {
        var names = ProductionNames.For(production);
        var database = production.Spec.Database;
        return [.. Enumerable.Range(0, database.Members).Select(member => Create(production, names, member))];
    }

    static V1Deployment Create(Production production, ProductionNames names, int member)
    {
        var ordinal = member.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var metadata = ProductionMetadata.Create(production, NameOf(names, member), Component);
        metadata.Labels[MemberLabel] = ordinal;
        var selector = new Dictionary<string, string>(metadata.Labels, StringComparer.Ordinal);
        var host = $"{names.MongoDB}-rs0-{ordinal}.{names.MongoDB}-rs0.{names.Namespace}.svc.cluster.local:27017";
        var metrics = production.Spec.Database.Metrics;

        return new()
        {
            ApiVersion = "apps/v1",
            Kind = "Deployment",
            Metadata = metadata,
            Spec = new()
            {
                Replicas = 1,
                Selector = new() { MatchLabels = selector },
                Template = new()
                {
                    Metadata = new()
                    {
                        Labels = selector,
                        Annotations = new Dictionary<string, string>(StringComparer.Ordinal)
                        {
                            ["prometheus.io/scrape"] = "true",
                            ["prometheus.io/port"] = ProductionDatabaseMetrics.Port.ToString(System.Globalization.CultureInfo.InvariantCulture),
                            ["prometheus.io/path"] = "/metrics"
                        }
                    },
                    Spec = new()
                    {
                        NodeSelector = metrics.NodeSelector.Count > 0 ? new Dictionary<string, string>(metrics.NodeSelector, StringComparer.Ordinal) : null,
                        SecurityContext = new() { RunAsNonRoot = true, RunAsUser = 65534, SeccompProfile = new() { Type = "RuntimeDefault" } },
                        Containers =
                        [
                            new()
                            {
                                Name = "exporter",
                                Image = metrics.ImageOrDefault(),
                                Args = Arguments(),
                                Ports = [new() { Name = "metrics", ContainerPort = ProductionDatabaseMetrics.Port }],
                                Env =
                                [
                                    new() { Name = "MONGODB_URI", Value = $"mongodb://{host}/admin?authSource=admin" },
                                    SecretEnvironment("MONGODB_USER", names.MongoDBUsers, UserKey),
                                    SecretEnvironment("MONGODB_PASSWORD", names.MongoDBUsers, PasswordKey)
                                ],
                                Resources = new()
                                {
                                    Requests = new Dictionary<string, ResourceQuantity>(StringComparer.Ordinal) { ["cpu"] = new("25m"), ["memory"] = new("48Mi") },
                                    Limits = new Dictionary<string, ResourceQuantity>(StringComparer.Ordinal) { ["memory"] = new("128Mi") }
                                },
                                SecurityContext = new() { AllowPrivilegeEscalation = false, ReadOnlyRootFilesystem = true, Capabilities = new() { Drop = ["ALL"] } }
                            }
                        ]
                    }
                }
            }
        };
    }

    static V1EnvVar SecretEnvironment(string name, string secret, string key) => new()
    {
        Name = name,
        ValueFrom = new() { SecretKeyRef = new() { Name = secret, Key = key } }
    };
}
