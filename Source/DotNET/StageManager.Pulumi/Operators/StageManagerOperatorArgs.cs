// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Pulumi;
using KubernetesProvider = Pulumi.Kubernetes.Provider;

namespace Cratis.Showrunner.StageManager.Pulumi.Operators;

/// <summary>
/// The inputs of a <see cref="StageManagerOperator"/>: the Stage Manager operator, installed with its Helm chart from inside the
/// component so that a consumer never writes a <c>Helm.V3.Release</c> of its own.
/// </summary>
public sealed class StageManagerOperatorArgs
{
    /// <summary>
    /// Gets the Kubernetes provider to create every resource with, or null for the default one.
    /// </summary>
    public KubernetesProvider? Provider { get; init; }

    /// <summary>
    /// Gets the Showrunner version. It is the version of the chart, of the operator image and of the Understudy image - and the tag
    /// <see cref="StageManagerOperator.WatchdogImage"/> gives for the watchdog image - because the three are released together. Use the
    /// version without a leading <c>v</c>, for example <c>2.17.1</c>.
    /// </summary>
    public required string Version { get; init; }

    /// <summary>
    /// Gets the name of the Helm release and so the prefix of the operator's Kubernetes resources. Defaults to <c>stage-manager</c>.
    /// </summary>
    public string ReleaseName { get; init; } = StageManagerOperator.DefaultReleaseName;

    /// <summary>
    /// Gets the namespace the operator runs in. Defaults to <c>stage-manager</c>.
    /// </summary>
    public string Namespace { get; init; } = StageManagerOperator.DefaultNamespace;

    /// <summary>
    /// Gets a value indicating whether the namespace is created. On by default; turn it off when the namespace is declared elsewhere.
    /// </summary>
    public bool CreateNamespace { get; init; } = true;

    /// <summary>
    /// Gets the namespaces the operator manages Performances and Productions in - the only namespaces where it may read and write
    /// Secrets and workloads. Every namespace must exist before the operator is installed: declare them in the stack and pass them in
    /// <see cref="DependsOn"/>. Empty means the operator's own namespace.
    /// </summary>
    public IList<string> ManagedNamespaces { get; init; } = [];

    /// <summary>
    /// Gets the Percona Operator for MongoDB to install alongside, or null when it is installed some other way or no Production uses
    /// a managed database.
    /// </summary>
    public StageManagerPerconaArgs? Percona { get; init; }

    /// <summary>
    /// Gets the repository of the operator image, or null for the published <c>cratis/showrunner-operator-image</c>. Use it to pull
    /// from a mirror; the tag is always <see cref="Version"/>.
    /// </summary>
    public string? OperatorImageRepository { get; init; }

    /// <summary>
    /// Gets the repository of the Understudy job image, or null for the published <c>cratis/showrunner-understudy-image</c>.
    /// </summary>
    public string? UnderstudyImageRepository { get; init; }

    /// <summary>
    /// Gets the repository of the watchdog image, or null for the published <c>cratis/showrunner-watchdog-image</c>. The operator does
    /// not run it; <see cref="StageManagerOperator.WatchdogImage"/> carries it with the tag of <see cref="Version"/> for a <c>Watchdog</c> component.
    /// </summary>
    public string? WatchdogImageRepository { get; init; }

    /// <summary>
    /// Gets the chart to install: an OCI reference, a repository chart name or a path. Defaults to the published
    /// <c>oci://registry-1.docker.io/cratis/showrunner-operator</c>. A path installs a chart from a checkout, which is how a chart that
    /// is not released yet is tried.
    /// </summary>
    public string? Chart { get; init; }

    /// <summary>
    /// Gets a value indicating whether the operator Deployment is waited for. On by default, so everything that depends on the
    /// component starts when the operator is available. The wait fails the update when the operator never becomes available.
    /// </summary>
    public bool WaitForOperator { get; init; } = true;

    /// <summary>
    /// Gets values of the chart that the typed inputs do not cover, merged <em>over</em> what the component sets. Use it rarely - for
    /// example <c>environments</c> for an Azure workload identity or <c>resources</c>. Nested maps are merged key by key.
    /// </summary>
    public IDictionary<string, object> Values { get; init; } = new Dictionary<string, object>(StringComparer.Ordinal);

    /// <summary>
    /// Gets the resources everything of the component depends on, such as the managed namespaces.
    /// </summary>
    public IList<Resource> DependsOn { get; init; } = [];
}
