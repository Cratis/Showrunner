// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Pulumi;
using KubernetesProvider = Pulumi.Kubernetes.Provider;

namespace Cratis.Showrunner.StageManager.Pulumi.Productions;

/// <summary>
/// The inputs of a <see cref="Production"/>.
/// </summary>
public sealed class ProductionArgs
{
    /// <summary>
    /// Gets the Kubernetes provider the resources are created with, or null for the default one.
    /// </summary>
    public KubernetesProvider? Provider { get; init; }

    /// <summary>
    /// Gets the Kubernetes name of the Production, or null to use the Pulumi name of the component. Applications
    /// reference it from their Performances.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Gets the namespace of the Production. It must be one of the operator chart's <c>managedNamespaces</c>. A Performance
    /// mounts the Chronicle connection Secret from its own namespace, so this is normally the application's.
    /// </summary>
    public required Input<string> Namespace { get; init; }

    /// <summary>
    /// Gets the tag of the <c>cratis/chronicle</c> image to run.
    /// </summary>
    public required Input<string> Version { get; init; }

    /// <summary>
    /// Gets the number of Chronicle silos. Defaults to 1.
    /// </summary>
    public int Replicas { get; init; } = 1;

    /// <summary>
    /// Gets the database of the Production.
    /// </summary>
    public required ProductionDatabaseArgs Database { get; init; }

    /// <summary>
    /// Gets how the key storage is provided, or null to leave Chronicle's keys in its database.
    /// </summary>
    public ProductionVaultArgs? Vault { get; init; }

    /// <summary>
    /// Gets how the Chronicle takes part in federation, or null for none.
    /// </summary>
    public ProductionFederationArgs? Federation { get; init; }

    /// <summary>
    /// Gets the Neo4j graph, or null for none.
    /// </summary>
    public ProductionGraphArgs? Graph { get; init; }

    /// <summary>
    /// Gets where the pods of the Production may run.
    /// </summary>
    public ProductionSchedulingArgs? Scheduling { get; init; }

    /// <summary>
    /// Gets the compute resources of the Chronicle container.
    /// </summary>
    public ProductionResourcesArgs? Resources { get; init; }

    /// <summary>
    /// Gets the named sinks, or null for none.
    /// </summary>
    public ProductionSinksArgs? Sinks { get; init; }

    /// <summary>
    /// Gets the labels put on the Production and on the Secrets the component writes.
    /// </summary>
    public IDictionary<string, string> Labels { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>
    /// Gets the resources the Production and its Secrets wait for, such as the namespace.
    /// </summary>
    public IList<Resource> DependsOn { get; init; } = [];
}
