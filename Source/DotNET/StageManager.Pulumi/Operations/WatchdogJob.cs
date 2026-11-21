// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Pulumi;
using Pulumi.Kubernetes.Types.Inputs.Rbac.V1;

namespace Cratis.Showrunner.StageManager.Pulumi.Operations;

/// <summary>
/// A job kind of the watchdog executable that runs as a CronJob of its own, with a ServiceAccount, rights and Secret of its own so
/// that what one job can do is never what another can.
/// </summary>
/// <param name="Kind">The kind, which is the command the executable runs: <c>runners</c>, <c>agent-harness</c>, <c>volumes</c> or <c>probe</c>.</param>
/// <param name="CheckName">The name of the check the job runs, which names its alert state.</param>
/// <param name="Schedule">The schedule.</param>
/// <param name="Environment">The variables that are not secret.</param>
/// <param name="Secrets">The secret variables, by name; they are written to a Secret of the job.</param>
/// <param name="ClusterRules">What the job reads or does cluster-wide.</param>
/// <param name="NamespacedRules">What the job reads or does in other namespaces, by namespace.</param>
/// <param name="StateConfigMaps">The ConfigMaps the job keeps its state in, in the namespace it runs in.</param>
internal sealed record WatchdogJob(
    string Kind,
    string CheckName,
    string Schedule,
    IReadOnlyList<KeyValuePair<string, string>> Environment,
    IReadOnlyDictionary<string, Input<string>> Secrets,
    IReadOnlyList<PolicyRuleArgs> ClusterRules,
    IReadOnlyDictionary<string, IReadOnlyList<PolicyRuleArgs>> NamespacedRules,
    IReadOnlyList<string> StateConfigMaps);
