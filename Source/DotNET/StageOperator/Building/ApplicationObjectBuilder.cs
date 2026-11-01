// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.StageOperator.Configuration;
using Cratis.Showrunner.StageOperator.Resources;
using k8s.Models;

namespace Cratis.Showrunner.StageOperator.Building;

/// <summary>
/// Builds the Kubernetes objects of an application from its spec. Pure functions, so every property of the
/// generated objects can be checked without a cluster.
/// </summary>
public static class ApplicationObjectBuilder
{
    static readonly ResourceQuantity _defaultCpuRequest = new("250m");
    static readonly ResourceQuantity _defaultMemoryRequest = new("512Mi");
    static readonly ResourceQuantity _defaultCpuLimit = new("1");
    static readonly ResourceQuantity _defaultMemoryLimit = new("1Gi");

    /// <summary>Builds every object of an application.</summary>
    /// <param name="application">The application.</param>
    /// <param name="image">The Stage image to run.</param>
    /// <param name="replicas">The number of replicas: 1 to run, 0 to stop.</param>
    /// <param name="exposed">Whether the Ingress exists.</param>
    /// <param name="options">The operator configuration.</param>
    /// <param name="package">The ConfigMap that delivers the package archive, or null when the image carries the package.</param>
    /// <returns>The objects.</returns>
    /// <exception cref="InvalidOperationException">The application has no authentication; it must be refused before objects are built.</exception>
    public static DesiredObjects Build(StageApplication application, string image, int replicas, bool exposed, StageOperatorOptions options, string? package = null)
    {
        var authentication = application.Spec.Authentication
            ?? throw new InvalidOperationException("An application without authentication must be refused before objects are built.");

        return new DesiredObjects(
            BuildDeployment(application, image, replicas, authentication, options, package),
            BuildMongoStatefulSet(application, replicas, options),
            BuildService(application),
            BuildMongoService(application),
            BuildDeniedService(application),
            exposed ? BuildIngress(application, options) : null,
            [BuildApplicationPolicy(application, authentication, options), BuildMongoPolicy(application)]);
    }

    static Dictionary<string, string> SelectorLabels(StageApplication application, string component) => new()
    {
        [ApplicationNames.InstanceLabel] = application.Name,
        [ApplicationNames.ComponentLabel] = component
    };

    static Dictionary<string, string> Labels(StageApplication application, string component)
    {
        var labels = SelectorLabels(application, component);
        labels["app.kubernetes.io/name"] = "stage-application";
        labels["app.kubernetes.io/instance"] = ApplicationNames.LabelValue(application.Name);
        labels["app.kubernetes.io/managed-by"] = "stage-operator";
        labels["app.kubernetes.io/component"] = component;
        labels[ApplicationNames.PublicationLabel] = ApplicationNames.LabelValue(application.Spec.PublicationId);
        labels[ApplicationNames.OrganizationLabel] = ApplicationNames.LabelValue(application.Spec.Organization);
        return labels;
    }

    static V1ObjectMeta Meta(StageApplication application, string name, string component) => new()
    {
        Name = name,
        NamespaceProperty = application.Namespace,
        Labels = Labels(application, component),
        OwnerReferences =
        [
            new V1OwnerReference
            {
                ApiVersion = application.ApiVersion,
                Kind = application.Kind,
                Name = application.Name,
                Uid = application.Metadata.Uid,
                Controller = true,
                BlockOwnerDeletion = true
            }
        ]
    };

    static V1Deployment BuildDeployment(StageApplication application, string image, int replicas, StageApplicationAuthentication authentication, StageOperatorOptions options, string? package)
    {
        var selector = SelectorLabels(application, ApplicationNames.AppComponent);
        var host = ApplicationNames.Host(application, options.BaseDomain);
        var notes = application.Spec.Notes;

        List<V1EnvVar> stageEnvironment =
        [
            new() { Name = "Stage__Mode", Value = "Published" },
            new() { Name = "Stage__Published__Authentication__Mode", Value = "TrustedProxy" },
            new() { Name = "Stage__Published__Notes__Enabled", Value = (notes?.Enabled ?? false) ? "true" : "false" },
            new() { Name = "Stage__Published__Storage__ConnectionString", Value = $"mongodb://{ApplicationNames.Mongo(application)}:{ApplicationNames.MongoPort}" },

            // Only the proxy in the same pod may assert who the user is; Stage strips principal headers from anyone else.
            new() { Name = "Stage__Published__TrustedProxies__0", Value = "127.0.0.1" },
            new() { Name = "Stage__Published__TrustedProxies__1", Value = "::1" },
            new() { Name = "AllowedHosts", Value = host },
            new() { Name = "ASPNETCORE_URLS", Value = $"http://127.0.0.1:{ApplicationNames.StagePort}" }
        ];

        List<V1Volume> volumes = [];
        List<V1VolumeMount> mounts = [];

        if (!string.IsNullOrEmpty(package))
        {
            // Mounting a ConfigMap needs no rights for the operator; the kubelet does it. Only the archive key is projected.
            volumes.Add(new V1Volume
            {
                Name = ApplicationNames.PackageVolume,
                ConfigMap = new V1ConfigMapVolumeSource
                {
                    Name = package,
                    Items = [new V1KeyToPath { Key = ApplicationNames.PackageArchiveFile, Path = ApplicationNames.PackageArchiveFile }]
                }
            });
            mounts.Add(new V1VolumeMount { Name = ApplicationNames.PackageVolume, MountPath = ApplicationNames.PackageMountPath, ReadOnlyProperty = true });
            stageEnvironment.Add(new V1EnvVar { Name = "Stage__Published__PackagePath", Value = $"{ApplicationNames.PackageMountPath}/{ApplicationNames.PackageArchiveFile}" });
        }

        if (HasCollector(notes))
        {
            // The secret is only referenced by name; the kubelet mounts it and Notes reads the file on every use.
            volumes.Add(new V1Volume
            {
                Name = ApplicationNames.NotesTokenVolume,
                Secret = new V1SecretVolumeSource
                {
                    SecretName = notes!.CollectorTokenSecretRef,
                    Items = [new V1KeyToPath { Key = ApplicationNames.NotesTokenKey, Path = ApplicationNames.NotesTokenKey }]
                }
            });
            mounts.Add(new V1VolumeMount { Name = ApplicationNames.NotesTokenVolume, MountPath = ApplicationNames.NotesTokenMountPath, ReadOnlyProperty = true });
            stageEnvironment.AddRange(
            [
                new V1EnvVar { Name = "Notes__Transport", Value = "Http" },
                new V1EnvVar { Name = "Notes__CollectorUrl", Value = notes.CollectorUrl },
                new V1EnvVar { Name = "Notes__CollectorTokenFile", Value = $"{ApplicationNames.NotesTokenMountPath}/{ApplicationNames.NotesTokenKey}" },
                new V1EnvVar { Name = "Notes__PublicationId", Value = application.Spec.PublicationId },
                new V1EnvVar { Name = "Notes__CollectorTenantId", Value = application.Spec.Organization },
                new V1EnvVar { Name = "Notes__ApplicationName", Value = application.Spec.PublishedName }
            ]);
        }

        var stage = new V1Container
        {
            Name = ApplicationNames.StageContainer,
            Image = image,

            // Stage binds to the loopback interface and the container declares no port: the only way to reach it is
            // through the proxy in the same pod.
            Env = stageEnvironment,
            EnvFrom = string.IsNullOrEmpty(notes?.ConfigSecretRef) ? null : [SecretSource(notes.ConfigSecretRef)],
            Resources = BuildResources(application.Spec.Resources),
            VolumeMounts = mounts.Count == 0 ? null : mounts,
            SecurityContext = SecurityContext(),
            ReadinessProbe = new V1Probe
            {
                TcpSocket = new V1TCPSocketAction { Port = ApplicationNames.StagePort },
                PeriodSeconds = 5,
                FailureThreshold = 3
            }
        };

        var proxy = new V1Container
        {
            Name = ApplicationNames.ProxyContainer,
            Image = authentication.ProxyImage,
            Ports = [new V1ContainerPort { Name = ApplicationNames.ProxyPortName, ContainerPort = authentication.Port, Protocol = "TCP" }],
            Env =
            [

                // Everything else the proxy needs (providers, the upstream service, cookie domain) comes from the
                // publisher's configuration secret, which knows the proxy it chose.
                new() { Name = "ASPNETCORE_URLS", Value = $"http://0.0.0.0:{authentication.Port}" }
            ],
            EnvFrom = [SecretSource(authentication.ConfigSecretRef)],
            SecurityContext = SecurityContext(),
            ReadinessProbe = new V1Probe
            {
                TcpSocket = new V1TCPSocketAction { Port = authentication.Port },
                PeriodSeconds = 5,
                FailureThreshold = 3
            }
        };

        return new V1Deployment
        {
            ApiVersion = "apps/v1",
            Kind = "Deployment",
            Metadata = Meta(application, application.Name, ApplicationNames.AppComponent),
            Spec = new V1DeploymentSpec
            {
                Replicas = replicas,
                ProgressDeadlineSeconds = Math.Max(60, (int)options.RolloutTimeout.TotalSeconds),
                RevisionHistoryLimit = 2,
                Selector = new V1LabelSelector { MatchLabels = selector },

                // A new pod must be ready before the old one goes, so a failing image never takes the running version down.
                Strategy = new V1DeploymentStrategy
                {
                    Type = "RollingUpdate",
                    RollingUpdate = new V1RollingUpdateDeployment { MaxSurge = 1, MaxUnavailable = 0 }
                },
                Template = new V1PodTemplateSpec
                {
                    Metadata = new V1ObjectMeta { Labels = Labels(application, ApplicationNames.AppComponent) },
                    Spec = new V1PodSpec
                    {
                        AutomountServiceAccountToken = false,
                        Containers = [stage, proxy],
                        Volumes = volumes.Count == 0 ? null : volumes
                    }
                }
            }
        };
    }

    static bool HasCollector(StageApplicationNotes? notes) =>
        notes is { Enabled: true } && !string.IsNullOrWhiteSpace(notes.CollectorUrl) && !string.IsNullOrWhiteSpace(notes.CollectorTokenSecretRef);

    static V1EnvFromSource SecretSource(string secret) => new() { SecretRef = new V1SecretEnvSource { Name = secret } };

    static V1SecurityContext SecurityContext() => new()
    {
        AllowPrivilegeEscalation = false,
        Capabilities = new V1Capabilities { Drop = ["ALL"] },
        SeccompProfile = new V1SeccompProfile { Type = "RuntimeDefault" }
    };

    static V1ResourceRequirements BuildResources(StageApplicationResources? resources) => new()
    {
        Requests = new Dictionary<string, ResourceQuantity>
        {
            ["cpu"] = Quantity(resources?.Requests?.Cpu, _defaultCpuRequest),
            ["memory"] = Quantity(resources?.Requests?.Memory, _defaultMemoryRequest)
        },
        Limits = new Dictionary<string, ResourceQuantity>
        {
            ["cpu"] = Quantity(resources?.Limits?.Cpu, _defaultCpuLimit),
            ["memory"] = Quantity(resources?.Limits?.Memory, _defaultMemoryLimit)
        }
    };

    static ResourceQuantity Quantity(string? value, ResourceQuantity fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : new ResourceQuantity(value);

    static V1StatefulSet BuildMongoStatefulSet(StageApplication application, int replicas, StageOperatorOptions options)
    {
        var selector = SelectorLabels(application, ApplicationNames.MongoComponent);
        var size = application.Spec.Storage?.Size;
        var claim = new V1PersistentVolumeClaim
        {
            Metadata = new V1ObjectMeta { Name = "data", Labels = Labels(application, ApplicationNames.MongoComponent) },
            Spec = new V1PersistentVolumeClaimSpec
            {
                AccessModes = ["ReadWriteOnce"],
                StorageClassName = string.IsNullOrEmpty(options.StorageClassName) ? null : options.StorageClassName,
                Resources = new V1VolumeResourceRequirements
                {
                    Requests = new Dictionary<string, ResourceQuantity>
                    {
                        ["storage"] = new ResourceQuantity(string.IsNullOrWhiteSpace(size) ? options.DefaultStorageSize : size)
                    }
                }
            }
        };

        return new V1StatefulSet
        {
            ApiVersion = "apps/v1",
            Kind = "StatefulSet",
            Metadata = Meta(application, ApplicationNames.Mongo(application), ApplicationNames.MongoComponent),
            Spec = new V1StatefulSetSpec
            {
                Replicas = replicas,
                ServiceName = ApplicationNames.Mongo(application),
                Selector = new V1LabelSelector { MatchLabels = selector },
                VolumeClaimTemplates = [claim],
                Template = new V1PodTemplateSpec
                {
                    Metadata = new V1ObjectMeta { Labels = Labels(application, ApplicationNames.MongoComponent) },
                    Spec = new V1PodSpec
                    {
                        AutomountServiceAccountToken = false,
                        Containers =
                        [
                            new V1Container
                            {
                                Name = "mongo",
                                Image = options.MongoImage,
                                Ports = [new V1ContainerPort { Name = "mongo", ContainerPort = ApplicationNames.MongoPort, Protocol = "TCP" }],
                                VolumeMounts = [new V1VolumeMount { Name = "data", MountPath = "/data/db" }],
                                SecurityContext = new V1SecurityContext
                                {
                                    AllowPrivilegeEscalation = false,
                                    SeccompProfile = new V1SeccompProfile { Type = "RuntimeDefault" }
                                },
                                ReadinessProbe = new V1Probe
                                {
                                    TcpSocket = new V1TCPSocketAction { Port = ApplicationNames.MongoPort },
                                    PeriodSeconds = 5
                                }
                            }
                        ]
                    }
                }
            }
        };
    }

    static V1Service BuildService(StageApplication application) => new()
    {
        ApiVersion = "v1",
        Kind = "Service",
        Metadata = Meta(application, application.Name, ApplicationNames.AppComponent),
        Spec = new V1ServiceSpec
        {
            Type = "ClusterIP",
            Selector = SelectorLabels(application, ApplicationNames.AppComponent),

            // The one port the Service exposes: the proxy. Stage and Chronicle are never routable.
            Ports = [new V1ServicePort { Name = ApplicationNames.ProxyPortName, Port = ApplicationNames.ServicePort, TargetPort = ApplicationNames.ProxyPortName, Protocol = "TCP" }]
        }
    };

    static V1Service BuildMongoService(StageApplication application) => new()
    {
        ApiVersion = "v1",
        Kind = "Service",
        Metadata = Meta(application, ApplicationNames.Mongo(application), ApplicationNames.MongoComponent),
        Spec = new V1ServiceSpec
        {
            ClusterIP = "None",
            Selector = SelectorLabels(application, ApplicationNames.MongoComponent),
            Ports = [new V1ServicePort { Name = "mongo", Port = ApplicationNames.MongoPort, Protocol = "TCP" }]
        }
    };

    static V1Service BuildDeniedService(StageApplication application) => new()
    {
        ApiVersion = "v1",
        Kind = "Service",
        Metadata = Meta(application, ApplicationNames.Denied(application), ApplicationNames.AppComponent),
        Spec = new V1ServiceSpec
        {
            Type = "ClusterIP",

            // No selector, so no endpoints: a request routed here is refused by the ingress controller.
            Ports = [new V1ServicePort { Name = ApplicationNames.ProxyPortName, Port = ApplicationNames.ServicePort, Protocol = "TCP" }]
        }
    };

    static V1Ingress BuildIngress(StageApplication application, StageOperatorOptions options)
    {
        var host = ApplicationNames.Host(application, options.BaseDomain);
        var annotations = new Dictionary<string, string>();

        string tlsSecret;
        if (!string.IsNullOrEmpty(options.TlsSecretName))
        {
            tlsSecret = options.TlsSecretName;
        }
        else
        {
            tlsSecret = $"{application.Name}-tls";
            if (!string.IsNullOrEmpty(options.TlsIssuerAnnotation))
            {
                annotations["cert-manager.io/cluster-issuer"] = options.TlsIssuerAnnotation;
            }
        }

        if (options.DenyPathsWithSnippet)
        {
            var pattern = string.Join('|', ApplicationNames.DeniedPaths.Select(path => path.TrimStart('/')));
            annotations["nginx.ingress.kubernetes.io/server-snippet"] = $"location ~* ^/({pattern})(/|$) {{ return 404; }}";
        }

        // Longest prefix wins in Ingress path matching, so the denied prefixes are answered by the Service
        // without endpoints before the catch-all reaches the proxy. This holds for every ingress controller.
        var denied = ApplicationNames.DeniedPaths.Select(path => new V1HTTPIngressPath
        {
            Path = path,
            PathType = "Prefix",
            Backend = new V1IngressBackend { Service = new V1IngressServiceBackend { Name = ApplicationNames.Denied(application), Port = new V1ServiceBackendPort { Number = ApplicationNames.ServicePort } } }
        });

        var allowed = new V1HTTPIngressPath
        {
            Path = "/",
            PathType = "Prefix",
            Backend = new V1IngressBackend { Service = new V1IngressServiceBackend { Name = application.Name, Port = new V1ServiceBackendPort { Name = ApplicationNames.ProxyPortName } } }
        };

        var metadata = Meta(application, application.Name, ApplicationNames.AppComponent);
        metadata.Annotations = annotations;

        return new V1Ingress
        {
            ApiVersion = "networking.k8s.io/v1",
            Kind = "Ingress",
            Metadata = metadata,
            Spec = new V1IngressSpec
            {
                IngressClassName = string.IsNullOrEmpty(options.IngressClassName) ? null : options.IngressClassName,
                Tls = [new V1IngressTLS { Hosts = [host], SecretName = tlsSecret }],
                Rules =
                [
                    new V1IngressRule
                    {
                        Host = host,
                        Http = new V1HTTPIngressRuleValue { Paths = [.. denied, allowed] }
                    }
                ]
            }
        };
    }

    static V1NetworkPolicy BuildApplicationPolicy(StageApplication application, StageApplicationAuthentication authentication, StageOperatorOptions options)
    {
        List<V1NetworkPolicyPort> ports = [new() { Port = authentication.Port, Protocol = "TCP" }];
        if ((application.Spec.Notes?.Enabled ?? false) && options.NotesCollectorPort > 0)
        {
            ports.Add(new V1NetworkPolicyPort { Port = options.NotesCollectorPort, Protocol = "TCP" });
        }

        return new V1NetworkPolicy
        {
            ApiVersion = "networking.k8s.io/v1",
            Kind = "NetworkPolicy",
            Metadata = Meta(application, application.Name, ApplicationNames.AppComponent),
            Spec = new V1NetworkPolicySpec
            {
                PodSelector = new V1LabelSelector { MatchLabels = SelectorLabels(application, ApplicationNames.AppComponent) },
                PolicyTypes = ["Ingress"],
                Ingress = [new V1NetworkPolicyIngressRule { Ports = ports }]
            }
        };
    }

    static V1NetworkPolicy BuildMongoPolicy(StageApplication application) => new()
    {
        ApiVersion = "networking.k8s.io/v1",
        Kind = "NetworkPolicy",
        Metadata = Meta(application, ApplicationNames.Mongo(application), ApplicationNames.MongoComponent),
        Spec = new V1NetworkPolicySpec
        {
            PodSelector = new V1LabelSelector { MatchLabels = SelectorLabels(application, ApplicationNames.MongoComponent) },
            PolicyTypes = ["Ingress"],
            Ingress =
            [
                new V1NetworkPolicyIngressRule
                {
                    FromProperty = [new V1NetworkPolicyPeer { PodSelector = new V1LabelSelector { MatchLabels = SelectorLabels(application, ApplicationNames.AppComponent) } }],
                    Ports = [new V1NetworkPolicyPort { Port = ApplicationNames.MongoPort, Protocol = "TCP" }]
                }
            ]
        }
    };
}
