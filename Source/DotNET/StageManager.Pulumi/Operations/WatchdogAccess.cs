// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Pulumi.Kubernetes.Types.Inputs.Rbac.V1;

namespace Cratis.Showrunner.StageManager.Pulumi.Operations;

/// <summary>
/// What a <see cref="Watchdog"/> is allowed to do: the minimum read rights of the checks that run, and write access to nothing but
/// its own state ConfigMap. Every rule is read-only (<c>get</c>, <c>list</c>), except the three verbs on that one ConfigMap.
/// </summary>
internal static class WatchdogAccess
{
    /// <summary>
    /// Gets the cluster-wide rules the checks need.
    /// </summary>
    /// <param name="checks">The checks that run.</param>
    /// <returns>The rules; empty when no check reads cluster-wide.</returns>
    internal static IReadOnlyList<PolicyRuleArgs> ClusterRules(IReadOnlyList<string> checks)
    {
        var rules = new List<PolicyRuleArgs>();
        bool Runs(params string[] names) => names.Any(checks.Contains);

        void Add(string group, string resource, params string[] verbs) =>
            rules.Add(new PolicyRuleArgs { ApiGroups = [group], Resources = [resource], Verbs = verbs });

        if (Runs(WatchdogChecks.Pods, WatchdogChecks.Volumes, WatchdogChecks.RestoreDrill, WatchdogChecks.Readiness))
        {
            Add(string.Empty, "pods", "list");
        }

        if (Runs(WatchdogChecks.Nodes, WatchdogChecks.Volumes))
        {
            Add(string.Empty, "nodes", "list");
        }

        if (Runs(WatchdogChecks.Volumes))
        {
            // The kubelet's stats summary, read through the API server, is where volume usage comes from.
            Add(string.Empty, "nodes/proxy", "get");
            Add(string.Empty, "persistentvolumeclaims", "list");
        }

        if (Runs(WatchdogChecks.Certificates))
        {
            Add("cert-manager.io", "certificates", "list");
        }

        return rules;
    }

    /// <summary>
    /// Gets the rules in the namespace the watchdog runs in: its state ConfigMap and its own Jobs.
    /// </summary>
    /// <param name="kubernetesName">The Kubernetes name of the watchdog.</param>
    /// <returns>The rules.</returns>
    internal static IReadOnlyList<PolicyRuleArgs> OwnNamespaceRules(string kubernetesName) =>
    [
        new PolicyRuleArgs { ApiGroups = [string.Empty], Resources = ["configmaps"], ResourceNames = [$"{kubernetesName}-state"], Verbs = ["get", "patch"] },

        // A ConfigMap cannot be created by name restriction, so creating it is its own rule; the watchdog creates it once.
        new PolicyRuleArgs { ApiGroups = [string.Empty], Resources = ["configmaps"], Verbs = ["create"] },
        new PolicyRuleArgs { ApiGroups = ["batch"], Resources = ["jobs"], Verbs = ["list"] },
        new PolicyRuleArgs { ApiGroups = ["batch"], Resources = ["cronjobs"], Verbs = ["get"] }
    ];

    /// <summary>
    /// Gets the read rules in another namespace that a check looks at.
    /// </summary>
    /// <param name="checks">The checks that run.</param>
    /// <param name="args">The inputs.</param>
    /// <returns>The rules by namespace.</returns>
    internal static IReadOnlyDictionary<string, IReadOnlyList<PolicyRuleArgs>> NamespacedRules(IReadOnlyList<string> checks, WatchdogArgs args)
    {
        var rules = new Dictionary<string, List<PolicyRuleArgs>>(StringComparer.Ordinal);

        void Add(string @namespace, string group, string resource, params string[] verbs)
        {
            if (!rules.TryGetValue(@namespace, out var list))
            {
                rules[@namespace] = list = [];
            }

            list.Add(new PolicyRuleArgs { ApiGroups = [group], Resources = [resource], Verbs = verbs });
        }

        if (checks.Contains(WatchdogChecks.CronJobs))
        {
            foreach (var job in args.CronJobs.Where(job => job.Namespace is not null))
            {
                Add(job.Namespace!, "batch", "cronjobs", "get");
            }
        }

        if (checks.Contains(WatchdogChecks.PbmBackups) && args.Pbm is { } pbm)
        {
            Add(pbm.Namespace, "psmdb.percona.com", "perconaservermongodbbackups", "list");
        }

        if (checks.Contains(WatchdogChecks.RestoreDrill) && args.RestoreDrill is { } drill)
        {
            Add(drill.Namespace, "batch", "cronjobs", "get");
            Add(drill.Namespace, "batch", "jobs", "list");
        }

        return rules.ToDictionary(pair => pair.Key, pair => (IReadOnlyList<PolicyRuleArgs>)pair.Value, StringComparer.Ordinal);
    }
}
