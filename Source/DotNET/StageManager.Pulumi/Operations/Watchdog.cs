// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Pulumi;
using Pulumi.Kubernetes.Batch.V1;
using Pulumi.Kubernetes.Core.V1;
using Pulumi.Kubernetes.Rbac.V1;
using Pulumi.Kubernetes.Types.Inputs.Batch.V1;
using Pulumi.Kubernetes.Types.Inputs.Core.V1;
using Pulumi.Kubernetes.Types.Inputs.Meta.V1;
using Pulumi.Kubernetes.Types.Inputs.Rbac.V1;

namespace Cratis.Showrunner.StageManager.Pulumi.Operations;

/// <summary>
/// The Showrunner watchdog of a deployment: a CronJob that runs the <c>Cratis.Showrunner.Operations</c> checks, with the
/// ServiceAccount and the minimum read rights they need, the Secrets of the alert channels, and a suspended CronJob that
/// delivers a test alert on demand. Nothing about the deployment is assumed: every namespace, workload and URL is an input.
/// </summary>
public sealed class Watchdog : ComponentResource
{
    /// <summary>
    /// The Pulumi resource type token of the component.
    /// </summary>
    public const string TypeToken = "cratis:showrunner:Watchdog";

    /// <summary>
    /// The schedule used when none is given: every five minutes.
    /// </summary>
    public const string DefaultSchedule = "*/5 * * * *";

    /// <summary>
    /// The run deadline used when none is given, in seconds.
    /// </summary>
    public const int DefaultDeadlineSeconds = 240;

    /// <summary>
    /// The image used when none is given.
    /// </summary>
    public const string DefaultImage = "cratis/showrunner-watchdog-image:latest";

    /// <summary>
    /// The SMTP port used when none is given.
    /// </summary>
    public const int DefaultSmtpPort = 587;

    const string SecretMountName = "tmp";

    /// <summary>
    /// Initializes a new instance of the <see cref="Watchdog"/> class.
    /// </summary>
    /// <param name="name">The Pulumi name of the component.</param>
    /// <param name="args">The inputs.</param>
    /// <param name="options">The component options.</param>
    public Watchdog(string name, WatchdogArgs args, ComponentResourceOptions? options = null)
        : base(TypeToken, name, options)
    {
        KubernetesName = args.Name ?? name;
        var labels = args.Labels.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        labels.TryAdd("app.kubernetes.io/name", KubernetesName);
        var checks = WatchdogChecks.For(args);
        var resourceOptions = new CustomResourceOptions
        {
            Parent = this,
            Provider = args.Provider,
            DependsOn = [.. args.DependsOn]
        };

        ObjectMetaArgs Metadata(string kubernetesName, bool namespaced = true)
        {
            var metadata = new ObjectMetaArgs { Name = kubernetesName };
            if (namespaced)
            {
                metadata.Namespace = args.Namespace;
            }

            foreach (var (label, value) in labels)
            {
                metadata.Labels.Add(label, value);
            }

            return metadata;
        }

        var serviceAccount = new ServiceAccount(
            $"{name}-sa",
            new ServiceAccountArgs { Metadata = Metadata(KubernetesName), AutomountServiceAccountToken = true },
            resourceOptions);
        var subjects = new[] { new SubjectArgs { Kind = "ServiceAccount", Name = KubernetesName, Namespace = args.Namespace } };

        DeclareAccess(name, args, checks, Metadata, subjects, resourceOptions);
        var secretName = DeclareSecret(name, args, Metadata, resourceOptions);
        var template = BuildPodTemplate(args, checks, secretName, labels);

        _ = new CronJob(
            $"{name}-cronjob",
            new CronJobArgs
            {
                Metadata = Metadata(KubernetesName),
                Spec = CronJobSpec(args.Schedule ?? DefaultSchedule, false, args.DeadlineSeconds ?? DefaultDeadlineSeconds, template([])),
            },
            new CustomResourceOptions { Parent = this, Provider = args.Provider, DependsOn = [serviceAccount, .. args.DependsOn] });

        if (args.TestAlertJob)
        {
            _ = new CronJob(
                $"{name}-test-alert",
                new CronJobArgs
                {
                    Metadata = Metadata($"{KubernetesName}-test-alert"),
                    Spec = CronJobSpec("0 0 1 1 *", true, args.DeadlineSeconds ?? DefaultDeadlineSeconds, template(["test-alert"])),
                },
                new CustomResourceOptions { Parent = this, Provider = args.Provider, DependsOn = [serviceAccount, .. args.DependsOn] });
        }

        RegisterOutputs();
    }

    /// <summary>
    /// Gets the Kubernetes name of the watchdog. It names the ServiceAccount and the CronJob; the state ConfigMap is
    /// <c>NAME-state</c> and the delivery test CronJob is <c>NAME-test-alert</c>.
    /// </summary>
    public string KubernetesName { get; }

    static CronJobSpecArgs CronJobSpec(string schedule, bool suspend, int deadlineSeconds, PodTemplateSpecArgs template) => new()
    {
        Schedule = schedule,
        Suspend = suspend,
        ConcurrencyPolicy = "Forbid",
        StartingDeadlineSeconds = 120,
        SuccessfulJobsHistoryLimit = 1,

        // Failed Jobs are the record of a run that was killed; the next run reports each one once.
        FailedJobsHistoryLimit = 3,
        JobTemplate = new JobTemplateSpecArgs
        {
            Spec = new JobSpecArgs
            {
                BackoffLimit = 0,
                ActiveDeadlineSeconds = deadlineSeconds,
                Template = template
            }
        }
    };

    static bool SecretHolds(WatchdogArgs args, string key) => key switch
    {
        WatchdogEnvironment.DiscordKey => args.Discord is not null,
        WatchdogEnvironment.SmtpUsernameKey or WatchdogEnvironment.SmtpPasswordKey => args.Smtp is not null,
        WatchdogEnvironment.DirectTokenKey => args.Direct is not null,
        WatchdogEnvironment.HeartbeatKey => args.HeartbeatUrl is not null,
        _ => false
    };

    void DeclareAccess(
        string name,
        WatchdogArgs args,
        IReadOnlyList<string> checks,
        Func<string, bool, ObjectMetaArgs> metadata,
        SubjectArgs[] subjects,
        CustomResourceOptions resourceOptions)
    {
        var clusterRules = WatchdogAccess.ClusterRules(checks);
        if (clusterRules.Count > 0)
        {
            var role = new ClusterRole($"{name}-cluster-role", new ClusterRoleArgs { Metadata = metadata($"{KubernetesName}", false), Rules = [.. clusterRules] }, resourceOptions);
            _ = new ClusterRoleBinding(
                $"{name}-cluster-role-binding",
                new ClusterRoleBindingArgs
                {
                    Metadata = metadata($"{KubernetesName}", false),
                    RoleRef = new RoleRefArgs { ApiGroup = "rbac.authorization.k8s.io", Kind = "ClusterRole", Name = KubernetesName },
                    Subjects = subjects
                },
                new CustomResourceOptions { Parent = this, Provider = args.Provider, DependsOn = [role, .. args.DependsOn] });
        }

        var own = new Role($"{name}-role", new RoleArgs { Metadata = metadata(KubernetesName, true), Rules = [.. WatchdogAccess.OwnNamespaceRules(KubernetesName)] }, resourceOptions);
        _ = new RoleBinding(
            $"{name}-role-binding",
            new RoleBindingArgs
            {
                Metadata = metadata(KubernetesName, true),
                RoleRef = new RoleRefArgs { ApiGroup = "rbac.authorization.k8s.io", Kind = "Role", Name = KubernetesName },
                Subjects = subjects
            },
            new CustomResourceOptions { Parent = this, Provider = args.Provider, DependsOn = [own, .. args.DependsOn] });

        foreach (var (@namespace, rules) in WatchdogAccess.NamespacedRules(checks, args))
        {
            var other = new ObjectMetaArgs { Name = $"{KubernetesName}-reader", Namespace = @namespace };
            var suffix = $"{name}-{@namespace}";
            var reader = new Role($"{suffix}-role", new RoleArgs { Metadata = other, Rules = [.. rules] }, resourceOptions);
            _ = new RoleBinding(
                $"{suffix}-role-binding",
                new RoleBindingArgs
                {
                    Metadata = new ObjectMetaArgs { Name = $"{KubernetesName}-reader", Namespace = @namespace },
                    RoleRef = new RoleRefArgs { ApiGroup = "rbac.authorization.k8s.io", Kind = "Role", Name = $"{KubernetesName}-reader" },
                    Subjects = subjects
                },
                new CustomResourceOptions { Parent = this, Provider = args.Provider, DependsOn = [reader, .. args.DependsOn] });
        }
    }

    string? DeclareSecret(string name, WatchdogArgs args, Func<string, bool, ObjectMetaArgs> metadata, CustomResourceOptions resourceOptions)
    {
        var data = new InputMap<string>();
        var any = false;
        void Protect(string key, Input<string>? value)
        {
            if (value is not null)
            {
                data[key] = StageManagerSecrets.Protect(value);
                any = true;
            }
        }

        Protect(WatchdogEnvironment.DiscordKey, args.Discord?.WebhookUrl);
        Protect(WatchdogEnvironment.SmtpUsernameKey, args.Smtp?.Username);
        Protect(WatchdogEnvironment.SmtpPasswordKey, args.Smtp?.Password);
        Protect(WatchdogEnvironment.DirectTokenKey, args.Direct?.Token);
        Protect(WatchdogEnvironment.HeartbeatKey, args.HeartbeatUrl);
        if (!any)
        {
            return null;
        }

        var secretName = $"{KubernetesName}-channels";
        _ = new Secret($"{name}-channels", new SecretArgs { Metadata = metadata(secretName, true), StringData = data }, resourceOptions);
        return secretName;
    }

    Func<IEnumerable<string>, PodTemplateSpecArgs> BuildPodTemplate(WatchdogArgs args, IReadOnlyList<string> checks, string? secretName, IReadOnlyDictionary<string, string> labels)
    {
        var environment = new List<EnvVarArgs>();
        environment.AddRange(WatchdogEnvironment.Plain(KubernetesName, args, checks).Select(pair => new EnvVarArgs { Name = pair.Key, Value = pair.Value }));
        environment.Add(new EnvVarArgs { Name = "WATCHDOG_NAMESPACE", Value = args.Namespace });
        environment.Add(new EnvVarArgs { Name = "POD_NAMESPACE", ValueFrom = new EnvVarSourceArgs { FieldRef = new ObjectFieldSelectorArgs { FieldPath = "metadata.namespace" } } });

        // The Job's name, so a crash is reported once for it.
        environment.Add(new EnvVarArgs { Name = "WATCHDOG_JOB_NAME", ValueFrom = new EnvVarSourceArgs { FieldRef = new ObjectFieldSelectorArgs { FieldPath = "metadata.labels['batch.kubernetes.io/job-name']" } } });

        if (secretName is not null)
        {
            string[] keys = [WatchdogEnvironment.DiscordKey, WatchdogEnvironment.SmtpUsernameKey, WatchdogEnvironment.SmtpPasswordKey, WatchdogEnvironment.DirectTokenKey, WatchdogEnvironment.HeartbeatKey];
            environment.AddRange(keys.Where(key => SecretHolds(args, key)).Select(key => new EnvVarArgs
            {
                Name = key,
                ValueFrom = new EnvVarSourceArgs { SecretKeyRef = new SecretKeySelectorArgs { Name = secretName, Key = key } }
            }));
        }

        return commandArguments => new PodTemplateSpecArgs
        {
            Metadata = new ObjectMetaArgs { Labels = labels.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal) },
            Spec = new PodSpecArgs
            {
                ServiceAccountName = KubernetesName,
                RestartPolicy = "Never",
                SecurityContext = new PodSecurityContextArgs { RunAsNonRoot = true, SeccompProfile = new SeccompProfileArgs { Type = "RuntimeDefault" } },
                Containers =
                [
                    new ContainerArgs
                    {
                        Name = "watchdog",
                        Image = args.Image ?? DefaultImage,
                        Args = [.. commandArguments.DefaultIfEmpty("cluster")],
                        Env = environment,
                        SecurityContext = new SecurityContextArgs
                        {
                            AllowPrivilegeEscalation = false,
                            ReadOnlyRootFilesystem = true,
                            Capabilities = new CapabilitiesArgs { Drop = ["ALL"] }
                        },
                        VolumeMounts = [new VolumeMountArgs { Name = SecretMountName, MountPath = "/tmp" }]
                    }
                ],
                Volumes = [new VolumeArgs { Name = SecretMountName, EmptyDir = new EmptyDirVolumeSourceArgs() }]
            }
        };
    }
}
