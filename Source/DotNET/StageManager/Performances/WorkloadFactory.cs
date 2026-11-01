// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// Creates the Deployment and the Service of a workload. Every Service is ClusterIP: nothing but the Ingress to AuthProxy is reachable from outside.
/// </summary>
public static class WorkloadFactory
{
    /// <summary>
    /// Creates the Deployment of a workload.
    /// </summary>
    /// <param name="performance">The Performance that owns it.</param>
    /// <param name="workload">The workload.</param>
    /// <returns>The Deployment.</returns>
    public static V1Deployment Deployment(Performance performance, WorkloadDefinition workload)
    {
        var labels = PerformanceMetadata.Labels(performance, workload.Component);
        var volumes = new List<V1Volume>();
        var mounts = new List<V1VolumeMount>();

        foreach (var mount in workload.SecretMounts)
        {
            volumes.Add(new() { Name = mount.VolumeName, Projected = new() { DefaultMode = 0x120, Sources = [.. mount.Sources.Select(ProjectedSource)] } });
            mounts.Add(new() { Name = mount.VolumeName, MountPath = mount.MountPath, ReadOnlyProperty = true });
        }

        foreach (var mount in workload.ConfigMapMounts)
        {
            volumes.Add(new() { Name = mount.VolumeName, ConfigMap = new() { Name = mount.ConfigMapName } });
            mounts.Add(new() { Name = mount.VolumeName, MountPath = mount.MountPath, ReadOnlyProperty = true });
        }

        return new()
        {
            ApiVersion = "apps/v1",
            Kind = "Deployment",
            Metadata = PerformanceMetadata.Create(performance, workload.Name, workload.Component),
            Spec = new()
            {
                Replicas = workload.Replicas,
                Selector = new() { MatchLabels = labels },
                Template = new()
                {
                    Metadata = new()
                    {
                        Labels = labels,
                        Annotations = new Dictionary<string, string> { [PerformanceMetadata.ConfigRevisionAnnotation] = workload.ConfigRevision }
                    },
                    Spec = new()
                    {
                        AutomountServiceAccountToken = false,
                        SecurityContext = new() { RunAsNonRoot = true, FsGroup = 10001, SeccompProfile = new() { Type = "RuntimeDefault" } },
                        Volumes = volumes,
                        Containers =
                        [
                            new()
                            {
                                Name = workload.Component,
                                Image = workload.Image,
                                Ports = [new() { Name = "http", ContainerPort = workload.Port }],
                                Env = [.. workload.Environment.OrderBy(pair => pair.Key, StringComparer.Ordinal).Select(pair => new V1EnvVar { Name = pair.Key, Value = pair.Value })],
                                VolumeMounts = mounts,
                                ReadinessProbe = new() { TcpSocket = new() { Port = "http" }, PeriodSeconds = 5 },
                                SecurityContext = new() { AllowPrivilegeEscalation = false, Capabilities = new() { Drop = ["ALL"] } }
                            }
                        ]
                    }
                }
            }
        };
    }

    /// <summary>
    /// Creates the ClusterIP Service of a workload.
    /// </summary>
    /// <param name="performance">The Performance that owns it.</param>
    /// <param name="workload">The workload.</param>
    /// <returns>The Service.</returns>
    public static V1Service Service(Performance performance, WorkloadDefinition workload) => new()
    {
        ApiVersion = "v1",
        Kind = "Service",
        Metadata = PerformanceMetadata.Create(performance, workload.Name, workload.Component),
        Spec = new()
        {
            Type = "ClusterIP",
            Selector = PerformanceMetadata.Labels(performance, workload.Component),
            Ports = [new() { Name = "http", Port = workload.Port, TargetPort = "http" }]
        }
    };

    static V1VolumeProjection ProjectedSource(SecretSource source) => new()
    {
        Secret = new()
        {
            Name = source.SecretName,
            Items = source.Items?.Select(item => new V1KeyToPath { Key = item.Key, Path = item.Value }).ToList()
        }
    };
}
