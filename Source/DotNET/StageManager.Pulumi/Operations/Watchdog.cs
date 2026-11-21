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
        var template = BuildPodTemplate(args, checks, secretName, labels, null, null);

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

        foreach (var job in WatchdogJobs.Plan(KubernetesName, args))
        {
            DeclareJob(name, args, job, secretName, labels, Metadata, resourceOptions);
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
        WatchdogEnvironment.ChronicleSecretKey => args.Chronicle is not null,
        WatchdogEnvironment.ChronicleAuthorityKey => args.Chronicle?.CertificateAuthority is not null,
        WatchdogEnvironment.GitHubAuditKey => args.GitHubAudit is not null,
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

    void DeclareJob(
        string name,
        WatchdogArgs args,
        WatchdogJob job,
        string? channelsSecret,
        IReadOnlyDictionary<string, string> labels,
        Func<string, bool, ObjectMetaArgs> metadata,
        CustomResourceOptions resourceOptions)
    {
        var jobName = $"{KubernetesName}-{job.Kind}";
        var suffix = $"{name}-{job.Kind}";
        var serviceAccount = new ServiceAccount(
            $"{suffix}-sa",
            new ServiceAccountArgs { Metadata = metadata(jobName, true), AutomountServiceAccountToken = true },
            resourceOptions);
        var subjects = new[] { new SubjectArgs { Kind = "ServiceAccount", Name = jobName, Namespace = args.Namespace } };
        var dependsOn = new CustomResourceOptions { Parent = this, Provider = args.Provider, DependsOn = [serviceAccount, .. args.DependsOn] };

        // What the job is allowed to do in its own namespace: keep its own alert state and its own runner state, see its own Jobs.
        string[] stateMaps = [$"{KubernetesName}-state-{job.CheckName}", .. job.StateConfigMaps];
        var ownRules = new List<PolicyRuleArgs>
        {
            new() { ApiGroups = [string.Empty], Resources = ["configmaps"], ResourceNames = [.. stateMaps], Verbs = ["get", "patch"] },
            new() { ApiGroups = [string.Empty], Resources = ["configmaps"], Verbs = ["create"] },
            new() { ApiGroups = ["batch"], Resources = ["jobs"], Verbs = ["list"] },
            new() { ApiGroups = ["batch"], Resources = ["cronjobs"], ResourceNames = [jobName], Verbs = ["get"] }
        };
        Bind(suffix, args, metadata(jobName, true), "Role", [.. ownRules], subjects, resourceOptions, serviceAccount);
        foreach (var (@namespace, rules) in job.NamespacedRules)
        {
            Bind($"{suffix}-{@namespace}", args, new ObjectMetaArgs { Name = $"{jobName}-reader", Namespace = @namespace }, "Role", [.. rules], subjects, resourceOptions, serviceAccount);
        }

        if (job.ClusterRules.Count > 0)
        {
            Bind($"{suffix}-cluster", args, new ObjectMetaArgs { Name = jobName }, "ClusterRole", [.. job.ClusterRules], subjects, resourceOptions, serviceAccount);
        }

        string? jobSecret = null;
        if (job.Secrets.Count > 0)
        {
            var data = new InputMap<string>();
            foreach (var (key, value) in job.Secrets)
            {
                data[key] = StageManagerSecrets.Protect(value);
            }

            jobSecret = $"{jobName}-credentials";
            _ = new Secret($"{suffix}-credentials", new SecretArgs { Metadata = metadata(jobSecret, true), StringData = data }, resourceOptions);
        }

        var template = BuildPodTemplate(args, [job.CheckName], channelsSecret, labels, job, jobSecret);
        _ = new CronJob(
            $"{suffix}-cronjob",
            new CronJobArgs
            {
                Metadata = metadata(jobName, true),
                Spec = CronJobSpec(job.Schedule, false, args.DeadlineSeconds ?? DefaultDeadlineSeconds, template([job.Kind]))
            },
            dependsOn);
    }

    void Bind(
        string name,
        WatchdogArgs args,
        ObjectMetaArgs metadata,
        string kind,
        PolicyRuleArgs[] rules,
        SubjectArgs[] subjects,
        CustomResourceOptions resourceOptions,
        ServiceAccount serviceAccount)
    {
        var bindingMetadata = new ObjectMetaArgs { Name = metadata.Name, Namespace = metadata.Namespace };
        if (kind == "ClusterRole")
        {
            var role = new ClusterRole($"{name}-role", new ClusterRoleArgs { Metadata = metadata, Rules = rules }, resourceOptions);
            _ = new ClusterRoleBinding(
                $"{name}-role-binding",
                new ClusterRoleBindingArgs { Metadata = bindingMetadata, RoleRef = new RoleRefArgs { ApiGroup = "rbac.authorization.k8s.io", Kind = kind, Name = metadata.Name! }, Subjects = subjects },
                new CustomResourceOptions { Parent = this, Provider = args.Provider, DependsOn = [role, serviceAccount, .. args.DependsOn] });
            return;
        }

        var namespaced = new Role($"{name}-role", new RoleArgs { Metadata = metadata, Rules = rules }, resourceOptions);
        _ = new RoleBinding(
            $"{name}-role-binding",
            new RoleBindingArgs { Metadata = bindingMetadata, RoleRef = new RoleRefArgs { ApiGroup = "rbac.authorization.k8s.io", Kind = kind, Name = metadata.Name! }, Subjects = subjects },
            new CustomResourceOptions { Parent = this, Provider = args.Provider, DependsOn = [namespaced, serviceAccount, .. args.DependsOn] });
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
        Protect(WatchdogEnvironment.ChronicleSecretKey, args.Chronicle?.ClientSecret);
        Protect(WatchdogEnvironment.ChronicleAuthorityKey, args.Chronicle?.CertificateAuthority);
        Protect(WatchdogEnvironment.GitHubAuditKey, args.GitHubAudit?.Token);
        if (!any)
        {
            return null;
        }

        var secretName = $"{KubernetesName}-channels";
        _ = new Secret($"{name}-channels", new SecretArgs { Metadata = metadata(secretName, true), StringData = data }, resourceOptions);
        return secretName;
    }

    Func<IEnumerable<string>, PodTemplateSpecArgs> BuildPodTemplate(WatchdogArgs args, IReadOnlyList<string> checks, string? secretName, IReadOnlyDictionary<string, string> labels, WatchdogJob? job, string? jobSecretName)
    {
        var serviceAccount = job is null ? KubernetesName : $"{KubernetesName}-{job.Kind}";
        var environment = new List<EnvVarArgs>();
        var plain = WatchdogEnvironment.Plain(KubernetesName, args, checks);
        if (job is null)
        {
            environment.AddRange(plain.Select(pair => new EnvVarArgs { Name = pair.Key, Value = pair.Value }));
        }
        else
        {
            // A job kind delivers through the same channels with the same timing, and nothing else of the cluster run's settings.
            var shared = plain.Where(pair => WatchdogEnvironment.Shared.Contains(pair.Key) || pair.Key == "WATCHDOG_STATE_CONFIGMAP");
            environment.AddRange(shared.Concat(job.Environment).Append(new("WATCHDOG_CRONJOB", serviceAccount)).Append(new("WATCHDOG_SCHEDULE", job.Schedule)).Select(pair => new EnvVarArgs { Name = pair.Key, Value = pair.Value }));
            environment.AddRange(job.Secrets.Keys.Select(key => new EnvVarArgs
            {
                Name = key,
                ValueFrom = new EnvVarSourceArgs { SecretKeyRef = new SecretKeySelectorArgs { Name = jobSecretName!, Key = key } }
            }));
        }

        environment.Add(new EnvVarArgs { Name = "WATCHDOG_NAMESPACE", Value = args.Namespace });
        environment.Add(new EnvVarArgs { Name = "POD_NAMESPACE", ValueFrom = new EnvVarSourceArgs { FieldRef = new ObjectFieldSelectorArgs { FieldPath = "metadata.namespace" } } });

        // The Job's name, so a crash is reported once for it.
        environment.Add(new EnvVarArgs { Name = "WATCHDOG_JOB_NAME", ValueFrom = new EnvVarSourceArgs { FieldRef = new ObjectFieldSelectorArgs { FieldPath = "metadata.labels['batch.kubernetes.io/job-name']" } } });

        if (secretName is not null)
        {
            string[] keys = job is null
                ? [WatchdogEnvironment.DiscordKey, WatchdogEnvironment.SmtpUsernameKey, WatchdogEnvironment.SmtpPasswordKey, WatchdogEnvironment.DirectTokenKey, WatchdogEnvironment.HeartbeatKey, WatchdogEnvironment.ChronicleSecretKey, WatchdogEnvironment.ChronicleAuthorityKey, WatchdogEnvironment.GitHubAuditKey]
                : [WatchdogEnvironment.DiscordKey, WatchdogEnvironment.SmtpUsernameKey, WatchdogEnvironment.SmtpPasswordKey, WatchdogEnvironment.DirectTokenKey, WatchdogEnvironment.HeartbeatKey];
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
                ServiceAccountName = serviceAccount,
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
