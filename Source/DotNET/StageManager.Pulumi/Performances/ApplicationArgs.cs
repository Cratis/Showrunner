// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Pulumi;

namespace Cratis.Showrunner.StageManager.Pulumi.Performances;

/// <summary>
/// The application a Performance runs.
/// </summary>
public sealed class ApplicationArgs
{
    /// <summary>
    /// Gets the image of the application.
    /// </summary>
    public required Input<string> Image { get; init; }

    /// <summary>
    /// Gets the names of the application's event stores. The first one is the store Ante observes.
    /// </summary>
    public IList<string> EventStores { get; init; } = [];

    /// <summary>
    /// Gets the port the application listens on, or null for the operator's default (8080).
    /// </summary>
    public int? Port { get; init; }

    /// <summary>
    /// Gets the number of replicas, or null for the operator's default (1).
    /// </summary>
    public int? Replicas { get; init; }

    /// <summary>
    /// Gets the names of the sinks the application stores read models and graphs in. The Production must configure each one.
    /// </summary>
    public IList<string> Sinks { get; init; } = [];

    /// <summary>
    /// Gets a value indicating whether the application reads the graph of its Production with <c>Cratis.Arc.Neo4j</c>.
    /// </summary>
    public bool Graph { get; init; }

    /// <summary>
    /// Gets how a new version replaces the running one, or null for the cluster's default.
    /// </summary>
    public RolloutStrategy? Strategy { get; init; }

    /// <summary>
    /// Gets the names of the image pull Secrets of the application and its init containers.
    /// </summary>
    public IList<string> ImagePullSecrets { get; init; } = [];

    /// <summary>
    /// Gets the HTTP probes.
    /// </summary>
    public ApplicationProbesArgs? Probes { get; init; }

    /// <summary>
    /// Gets the CPU and memory requests and limits. None are set when it is left out.
    /// </summary>
    public ApplicationResourcesArgs? Resources { get; init; }

    /// <summary>
    /// Gets the node labels the pods must match.
    /// </summary>
    public IDictionary<string, string> NodeSelector { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>
    /// Gets the volumes of the pod.
    /// </summary>
    public IList<VolumeArgs> Volumes { get; init; } = [];

    /// <summary>
    /// Gets where the volumes are mounted in the application container.
    /// </summary>
    public IList<VolumeMountArgs> VolumeMounts { get; init; } = [];

    /// <summary>
    /// Gets the containers that run to completion, in order, before the application starts.
    /// </summary>
    public IList<InitContainerArgs> InitContainers { get; init; } = [];

    /// <summary>
    /// Gets the destinations the pods may connect to. Naming any isolates them for egress.
    /// </summary>
    public IList<EgressRuleArgs> Egress { get; init; } = [];

    /// <summary>
    /// Builds the spec of the application.
    /// </summary>
    /// <returns>The spec.</returns>
    internal Dictionary<string, object> ToSpec() =>
        SpecExtensions.NewSpec()
            .Put("image", Image)
            .PutList("eventStores", EventStores)
            .Put("port", Port)
            .Put("replicas", Replicas)
            .PutList("sinks", Sinks)
            .PutWhenTrue("graph", Graph)
            .PutEnum("strategy", Strategy)
            .PutList("imagePullSecrets", ImagePullSecrets)
            .PutObject("probes", Probes?.ToSpec() ?? SpecExtensions.NewSpec())
            .PutObject("resources", Resources?.ToSpec() ?? SpecExtensions.NewSpec())
            .PutMap("nodeSelector", NodeSelector.Select(pair => new KeyValuePair<string, object>(pair.Key, pair.Value)))
            .PutList("volumes", Volumes.Select(volume => volume.ToSpec()))
            .PutList("volumeMounts", VolumeMounts.Select(mount => mount.ToSpec()))
            .PutList("initContainers", InitContainers.Select(container => container.ToSpec()))
            .PutList("egress", Egress.Select(rule => rule.ToSpec()));
}
