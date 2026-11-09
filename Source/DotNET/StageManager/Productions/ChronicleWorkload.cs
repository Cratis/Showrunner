// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.StageManager.Performances;

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// Creates the StatefulSet that runs Chronicle and its headless Service. The Service's port is named <c>chronicle</c>,
/// so DNS answers <c>_chronicle._tcp</c> SRV queries and applications find every ready node through <c>chronicle+srv://</c>.
/// </summary>
public static class ChronicleWorkload
{
    /// <summary>
    /// The image Chronicle runs from: the production image, which requires a TLS certificate.
    /// </summary>
    public const string Image = "cratis/chronicle";

    /// <summary>
    /// The port applications, the Workbench and the health checks use.
    /// </summary>
    public const int Port = 35000;

    /// <summary>
    /// The port the nodes of a cluster talk to each other on.
    /// </summary>
    public const int SiloPort = 11111;

    /// <summary>
    /// The port of the Orleans client gateway.
    /// </summary>
    public const int GatewayPort = 30000;

    /// <summary>
    /// Where the TLS certificate is mounted.
    /// </summary>
    public const string CertificateDirectory = "/run/secrets/chronicle-tls";

    const string Component = "chronicle";
    const string Prefix = "Cratis__Chronicle__";
    const string CertificateVolume = "chronicle-tls";

    /// <summary>
    /// Creates the headless Service of Chronicle.
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
            Metadata = ProductionMetadata.Create(production, names.Chronicle, Component),
            Spec = new()
            {
                ClusterIP = "None",
                Selector = ProductionMetadata.Labels(production, Component),
                Ports =
                [
                    new() { Name = "chronicle", Protocol = "TCP", Port = Port, TargetPort = "chronicle" },
                    new() { Name = "silo", Protocol = "TCP", Port = SiloPort, TargetPort = "silo" },
                    new() { Name = "gateway", Protocol = "TCP", Port = GatewayPort, TargetPort = "gateway" }
                ]
            }
        };
    }

    /// <summary>
    /// Gets the clustering type of Chronicle: one node keeps the single-writer lease, more form one Orleans cluster through the database.
    /// </summary>
    /// <param name="spec">The spec of the Production.</param>
    /// <returns><c>MongoDB</c> or <c>Sql</c> above one replica, or null for one replica.</returns>
    public static string? ClusteringOf(ProductionSpec spec) => spec.Replicas switch
    {
        <= 1 => null,
        _ when StorageKinds.IsSql(spec.Database.Kind) => "Sql",
        _ => "MongoDB"
    };

    /// <summary>
    /// Creates the StatefulSet of Chronicle.
    /// </summary>
    /// <param name="production">The Production.</param>
    /// <param name="kernel">The kernel configuration of the Production.</param>
    /// <returns>The StatefulSet.</returns>
    public static V1StatefulSet StatefulSet(Production production, ChronicleKernelConfiguration kernel)
    {
        var names = ProductionNames.For(production);
        var spec = production.Spec;
        var labels = ProductionMetadata.Labels(production, Component);

        var environment = kernel.EnvironmentVariables
            .Append(new($"{Prefix}Tls__CertificatePath", $"{CertificateDirectory}/{ProductionCertificates.KeystoreKey}"))
            .Append(new($"{Prefix}Authentication__AdminUser__Username", ChronicleCredentials.AdminUserName))
            .OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .Select(pair => new V1EnvVar { Name = pair.Key, Value = pair.Value })
            .ToList();

        // Values that are secret come from their Secrets and never stand in the StatefulSet.
        environment.Add(FromSecret($"{Prefix}Tls__CertificatePassword", names.KeystorePassword, ProductionCertificates.KeystorePasswordKey));
        environment.Add(FromSecret($"{Prefix}Authentication__AdminUser__Password", names.Credentials, ChronicleCredentials.AdminPasswordKey));
        environment.Add(FromSecret($"{Prefix}Clients__0__ClientId", names.Credentials, ChronicleCredentials.ClientIdKey));
        environment.Add(FromSecret($"{Prefix}Clients__0__ClientSecret", names.Credentials, ChronicleCredentials.ClientSecretKey));

        if (ClusteringOf(spec) is { } clustering)
        {
            environment.Add(new() { Name = $"{Prefix}Clustering__Type", Value = clustering });
            environment.Add(new() { Name = $"{Prefix}Clustering__ClusterId", Value = names.Production });
            environment.Add(new() { Name = $"{Prefix}Clustering__ServiceId", Value = names.Production });
            environment.Add(new() { Name = $"{Prefix}Clustering__AdvertisedIP", ValueFrom = new() { FieldRef = new() { FieldPath = "status.podIP" } } });
        }

        var volumes = new List<V1Volume>
        {
            new()
            {
                Name = CertificateVolume,
                Secret = new() { SecretName = names.ChronicleTls, DefaultMode = 0x120, Items = [new() { Key = ProductionCertificates.KeystoreKey, Path = ProductionCertificates.KeystoreKey }] }
            }
        };
        var mounts = new List<V1VolumeMount> { new() { Name = CertificateVolume, MountPath = CertificateDirectory, ReadOnlyProperty = true } };

        if (kernel.Connections.Sources.Count > 0)
        {
            volumes.Add(new() { Name = kernel.Connections.VolumeName, Projected = new() { DefaultMode = 0x120, Sources = [.. kernel.Connections.Sources.Select(ProjectedSource)] } });
            mounts.Add(new() { Name = kernel.Connections.VolumeName, MountPath = kernel.Connections.MountPath, ReadOnlyProperty = true });
        }

        return new()
        {
            ApiVersion = "apps/v1",
            Kind = "StatefulSet",
            Metadata = ProductionMetadata.Create(production, names.Chronicle, Component),
            Spec = new()
            {
                Replicas = spec.Replicas,
                ServiceName = names.Chronicle,
                PodManagementPolicy = "Parallel",
                UpdateStrategy = new() { Type = "RollingUpdate" },
                Selector = new() { MatchLabels = labels },
                Template = new()
                {
                    Metadata = new() { Labels = labels },
                    Spec = new()
                    {
                        AutomountServiceAccountToken = false,
                        SecurityContext = new() { SeccompProfile = new() { Type = "RuntimeDefault" } },
                        Volumes = volumes,
                        Containers =
                        [
                            new()
                            {
                                Name = Component,
                                Image = $"{Image}:{spec.Version}",
                                Ports =
                                [
                                    new() { Name = "chronicle", ContainerPort = Port },
                                    new() { Name = "silo", ContainerPort = SiloPort },
                                    new() { Name = "gateway", ContainerPort = GatewayPort }
                                ],
                                Env = environment,
                                VolumeMounts = mounts,
                                StartupProbe = new() { TcpSocket = new() { Port = "chronicle" }, PeriodSeconds = 5, FailureThreshold = 60 },
                                LivenessProbe = new() { HttpGet = new() { Path = "/health/live", Port = "chronicle", Scheme = "HTTPS" }, PeriodSeconds = 10 },
                                ReadinessProbe = new() { HttpGet = new() { Path = "/health/ready", Port = "chronicle", Scheme = "HTTPS" }, PeriodSeconds = 5 },
                                SecurityContext = new() { AllowPrivilegeEscalation = false, Capabilities = new() { Drop = ["ALL"] } }
                            }
                        ]
                    }
                }
            }
        };
    }

    static V1EnvVar FromSecret(string name, string secret, string key) =>
        new() { Name = name, ValueFrom = new() { SecretKeyRef = new() { Name = secret, Key = key } } };

    static V1VolumeProjection ProjectedSource(SecretSource source) => new()
    {
        Secret = new()
        {
            Name = source.SecretName,
            Items = source.Items?.Select(item => new V1KeyToPath { Key = item.Key, Path = item.Value }).ToList()
        }
    };
}
