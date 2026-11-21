// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Pulumi;
using Pulumi.Kubernetes.Core.V1;
using Pulumi.Kubernetes.Types.Inputs.Core.V1;
using Pulumi.Kubernetes.Types.Inputs.Meta.V1;

namespace Cratis.Showrunner.StageManager.Pulumi.Performances;

/// <summary>
/// Declares a Stage Manager <c>Performance</c>, an application workload that runs against a Production, with the ConfigMap
/// that holds its Cratis environment file when the file is given as text.
/// </summary>
/// <remarks>
/// An application that runs a worker next to its service declares one Performance per component, all against the same
/// Production. Only one of them writes the environment ConfigMap; the others name it and depend on it.
/// </remarks>
public sealed class Performance : ComponentResource
{
    /// <summary>
    /// The Pulumi resource type token of the component.
    /// </summary>
    public const string TypeToken = "cratis:showrunner:Performance";

    /// <summary>
    /// Initializes a new instance of the <see cref="Performance"/> class.
    /// </summary>
    /// <param name="name">The Pulumi name of the component.</param>
    /// <param name="args">The inputs.</param>
    /// <param name="options">The component options.</param>
    public Performance(string name, PerformanceArgs args, ComponentResourceOptions? options = null)
        : base(TypeToken, name, options)
    {
        KubernetesName = args.Name ?? name;
        var labels = args.Labels.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        var environment = args.Environment;
        var dependsOn = new List<Resource>(args.DependsOn);

        var configMapName = environment.ConfigMapName
            ?? (environment.Content is null ? null : $"{KubernetesName}-environment")
            ?? throw new ArgumentException("Give the environment a ConfigMapName, or its Content to write one.", nameof(args));

        if (args.ProductionName is null && args.ChronicleConnectionSecret is null)
        {
            throw new ArgumentException("Name the Chronicle to use with ProductionName or ChronicleConnectionSecret.", nameof(args));
        }

        if (environment.Content is not null)
        {
            var metadata = new ObjectMetaArgs { Name = configMapName, Namespace = args.Namespace };
            foreach (var (label, value) in labels)
            {
                metadata.Labels.Add(label, value);
            }

            dependsOn.Add(new ConfigMap(
                $"{name}-environment",
                new ConfigMapArgs
                {
                    Metadata = metadata,
                    Data = { [environment.ConfigMapKey ?? environment.DefaultConfigMapKey] = environment.Content },
                },
                new CustomResourceOptions { Parent = this, Provider = args.Provider, DependsOn = [.. args.DependsOn] }));
        }

        var production = args.ProductionName is null
            ? null
            : SpecExtensions.NewSpec().Put("name", args.ProductionName).Put("namespace", args.ProductionNamespace);
        var chronicle = args.ChronicleConnectionSecret is null
            ? null
            : SpecExtensions.NewSpec().Put("connectionSecretRef", SpecExtensions.Reference(args.ChronicleConnectionSecret.Name, args.ChronicleConnectionSecret.Key));

        var spec = SpecExtensions.NewSpec()
            .Put("productionRef", production)
            .Put("chronicle", chronicle)
            .Put("application", args.Application.ToSpec())
            .Put("environment", environment.ToSpec(configMapName))
            .Put("lifetime", args.Lifetime?.ToSpec());

        StageManagerCustomResources.Declare(this, args.Provider, name, "Performance", KubernetesName, args.Namespace, labels, spec, dependsOn);

        RegisterOutputs();
    }

    /// <summary>
    /// Gets the Kubernetes name of the Performance.
    /// </summary>
    public string KubernetesName { get; }
}
