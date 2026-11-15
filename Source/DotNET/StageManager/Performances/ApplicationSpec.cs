// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// The application a <see cref="Performance"/> runs.
/// </summary>
public class ApplicationSpec
{
    /// <summary>
    /// The strategy that stops the old pod before it starts the new one.
    /// </summary>
    public const string Recreate = "Recreate";

    /// <summary>
    /// The strategy that starts new pods before it stops the old ones.
    /// </summary>
    public const string RollingUpdate = "RollingUpdate";

    /// <summary>
    /// Gets or sets the image of the rendered application.
    /// </summary>
    [Required]
    [Description("The image of the rendered application, or the Stage runtime image (cratis/stage:VERSION) when published is set.")]
    public string Image { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the names of the application's event stores.
    /// </summary>
    [Description("The names of the application's event stores. The first one is the store Ante observes.")]
    public IList<string> EventStores { get; set; } = [];

    /// <summary>
    /// Gets or sets the port the application listens on.
    /// </summary>
    [Description("The port the application listens on.")]
    public int Port { get; set; } = 8080;

    /// <summary>
    /// Gets or sets the number of replicas.
    /// </summary>
    [Description("The number of replicas.")]
    public int Replicas { get; set; } = 1;

    /// <summary>
    /// Gets or sets the names of the sinks the application stores read models and graphs in. The Production must configure each one.
    /// </summary>
    [Description("The names of the sinks a traditional application stores read models and graphs in. The Production named by productionRef must configure each one.")]
    public IList<string> Sinks { get; set; } = [];

    /// <summary>
    /// Gets or sets whether the application reads the graph of its Production with Cratis.Arc.Neo4j.
    /// </summary>
    [Description("Whether the application reads the graph of the Production named by productionRef with Cratis.Arc.Neo4j. The Performance then binds the Cratis:Neo4j settings to that graph. Using a Neo4j sink on the Production's managed graph implies it.")]
    public bool Graph { get; set; }

    /// <summary>
    /// Gets or sets the published Stage application settings. When set, the application runs as a published Stage application.
    /// </summary>
    [Description("Runs the application as a published Stage application: image is the Stage runtime image and the package comes from a ConfigMap.")]
    public PublishedSpec? Published { get; set; }

    /// <summary>
    /// Gets or sets how a new version replaces the running one.
    /// </summary>
    [Description("RollingUpdate (starts the new pod before it stops the old) or Recreate (stops the old pod first, so two never share state; use it for an application that hosts a single-replica silo). Empty keeps the cluster's default. Not used by a published application, which always recreates.")]
    public string Strategy { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the names of the Secrets the image is pulled with.
    /// </summary>
    [Description("The names of the image pull Secrets of the application and its init containers.")]
    public IList<string> ImagePullSecrets { get; set; } = [];

    /// <summary>
    /// Gets or sets the HTTP probes.
    /// </summary>
    [Description("The HTTP paths Kubernetes checks the application on. Not used by a published application.")]
    public ApplicationProbes Probes { get; set; } = new();

    /// <summary>
    /// Gets or sets the resources of the application container.
    /// </summary>
    [Description("The CPU and memory requests and limits of the application. None are set when it is left out. Not used by a published application; see published.resources.")]
    public ResourcesSpec? Resources { get; set; }

    /// <summary>
    /// Gets or sets the node labels the pods must match.
    /// </summary>
    [Description("The node labels the pods of the application must match.")]
    public IDictionary<string, string> NodeSelector { get; set; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>
    /// Gets or sets the volumes of the pod.
    /// </summary>
    [Description("The volumes of the application's pod. Not used by a published application.")]
    public IList<ApplicationVolume> Volumes { get; set; } = [];

    /// <summary>
    /// Gets or sets the mounts of the application container.
    /// </summary>
    [Description("Where the volumes are mounted in the application container.")]
    public IList<ApplicationVolumeMount> VolumeMounts { get; set; } = [];

    /// <summary>
    /// Gets or sets the init containers.
    /// </summary>
    [Description("Containers that run to completion, in order, before the application starts. Not used by a published application.")]
    public IList<ApplicationInitContainer> InitContainers { get; set; } = [];

    /// <summary>
    /// Gets or sets the destinations the application may connect to.
    /// </summary>
    [Description("The destinations the application's pods may connect to. Naming any isolates them for egress: they then reach DNS, their own namespace and these destinations only.")]
    public IList<ApplicationEgressRule> Egress { get; set; } = [];
}
