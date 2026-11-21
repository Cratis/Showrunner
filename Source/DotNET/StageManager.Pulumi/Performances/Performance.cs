// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Pulumi;
using Pulumi.Kubernetes.Core.V1;
using Pulumi.Kubernetes.Types.Inputs.Core.V1;
using Pulumi.Kubernetes.Types.Inputs.Meta.V1;

namespace Cratis.Showrunner.StageManager.Pulumi.Performances;

/// <summary>
/// Declares a Stage Manager <c>Performance</c>, an application workload that runs against a Production, with the ConfigMap
/// that holds its Cratis environment file when the file is given as text, and with the Secrets and ConfigMaps its AuthProxy
/// and Ante settings refer to when those are given as values.
/// </summary>
/// <remarks>
/// <para>
/// An application that runs a worker next to its service declares one Performance per component, all against the same
/// Production. Only one of them writes the environment ConfigMap; the others name it and depend on it.
/// </para>
/// <para>
/// AuthProxy, Ante and the Ingress are optional: declare <see cref="PerformanceArgs.AuthProxy"/>, <see cref="PerformanceArgs.Onboarding"/>
/// and <see cref="PerformanceArgs.Ingress"/> to run them. A Performance whose AuthProxy names an upstream is a gateway: it runs AuthProxy
/// in front of an existing Service and nothing else.
/// </para>
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
        var dependsOn = new List<Resource>(args.DependsOn);
        var gateway = args.AuthProxy?.Upstream is not null;

        if (!gateway && (args.Application is null || args.Environment is null))
        {
            throw new ArgumentException("Give the application and its environment, or an AuthProxy with an Upstream to declare a gateway.", nameof(args));
        }

        if (gateway && args.Environment is null && args.AuthProxy!.Providers.Any(provider => provider.ClientId is null && provider.CredentialsSecretName is null))
        {
            throw new ArgumentException("Give each provider of a gateway without an environment its ClientId and ClientSecret, or a CredentialsSecretName.", nameof(args));
        }

        if (!gateway && args.ProductionName is null && args.ChronicleConnectionSecret is null)
        {
            throw new ArgumentException("Name the Chronicle to use with ProductionName or ChronicleConnectionSecret.", nameof(args));
        }

        var environmentSpec = DeclareEnvironment(name, args, labels, dependsOn);

        var production = args.ProductionName is null
            ? null
            : SpecExtensions.NewSpec().Put("name", args.ProductionName).Put("namespace", args.ProductionNamespace);
        var chronicle = args.ChronicleConnectionSecret is null
            ? null
            : SpecExtensions.NewSpec().Put("connectionSecretRef", SpecExtensions.Reference(args.ChronicleConnectionSecret.Name, args.ChronicleConnectionSecret.Key));

        var context = new PerformanceContext(this, args.Provider, name, KubernetesName, args.Namespace, labels, [.. args.DependsOn]);
        var spec = SpecExtensions.NewSpec()
            .Put("productionRef", production)
            .Put("chronicle", chronicle)
            .Put("application", args.Application?.ToSpec())
            .Put("environment", environmentSpec)
            .Put("authProxy", args.AuthProxy?.ToSpec(context))
            .Put("onboarding", args.Onboarding?.ToSpec(context))
            .Put("ingress", args.Ingress?.ToSpec())
            .Put("signingKeys", args.SigningKeys?.ToSpec())
            .Put("lifetime", args.Lifetime?.ToSpec());

        // What the settings refer to has to exist before the operator reads it.
        dependsOn.AddRange(context.Declared);
        StageManagerCustomResources.Declare(this, args.Provider, name, "Performance", KubernetesName, args.Namespace, labels, spec, dependsOn);

        RegisterOutputs();
    }

    /// <summary>
    /// Gets the Kubernetes name of the Performance.
    /// </summary>
    public string KubernetesName { get; }

    Dictionary<string, object>? DeclareEnvironment(string name, PerformanceArgs args, Dictionary<string, string> labels, List<Resource> dependsOn)
    {
        if (args.Environment is not { } environment)
        {
            return null;
        }

        var configMapName = environment.ConfigMapName
            ?? (environment.Content is null ? null : $"{KubernetesName}-environment")
            ?? throw new ArgumentException("Give the environment a ConfigMapName, or its Content to write one.", nameof(args));

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

        return environment.ToSpec(configMapName);
    }
}
