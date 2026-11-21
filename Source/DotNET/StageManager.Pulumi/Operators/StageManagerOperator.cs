// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Pulumi;
using Pulumi.Kubernetes.Helm.V3;
using Pulumi.Kubernetes.Types.Inputs.Helm.V3;

namespace Cratis.Showrunner.StageManager.Pulumi.Operators;

/// <summary>
/// The Stage Manager operator, installed the way every Showrunner resource of this package is: from a Pulumi component, not from a
/// Helm release a consumer writes. One <see cref="StageManagerOperatorArgs.Version"/> drives the chart and the operator and Understudy
/// image tags (and the tag <see cref="WatchdogImage"/> carries for the watchdog); the managed namespaces, the optional Percona
/// Operator for MongoDB, image repository overrides and a values passthrough for the rare chart value are typed inputs.
/// </summary>
/// <remarks>
/// <para>Make everything that needs the operator - a <c>Production</c>, a <c>Performance</c>, an <c>Understudy</c> - depend on this
/// component (<c>DependsOn = [operator]</c>): the component finishes when the operator Deployment is available and the CRDs are installed, so
/// they never race it.</para>
/// <para>Helm installs the CRDs of a chart once and never upgrades them. When a release changes a CRD, apply the chart's <c>crds</c> folder
/// before updating the stack; see <c>Documentation/StageManager/install.md</c>.</para>
/// </remarks>
public sealed class StageManagerOperator : ComponentResource
{
    /// <summary>
    /// The type token of the component.
    /// </summary>
    public const string TypeToken = "cratis:showrunner:StageManagerOperator";

    /// <summary>
    /// The default name of the Helm release.
    /// </summary>
    public const string DefaultReleaseName = "stage-manager";

    /// <summary>
    /// The default namespace of the operator.
    /// </summary>
    public const string DefaultNamespace = "stage-manager";

    /// <summary>
    /// The published chart.
    /// </summary>
    public const string DefaultChart = "oci://registry-1.docker.io/cratis/showrunner-operator";

    /// <summary>
    /// The default namespace of the Percona operator.
    /// </summary>
    public const string DefaultPerconaNamespace = "percona-operator";

    /// <summary>
    /// The default Helm repository of the Percona charts.
    /// </summary>
    public const string DefaultPerconaRepository = "https://percona.github.io/percona-helm-charts/";

    /// <summary>
    /// The repository of the operator image when none is given.
    /// </summary>
    public const string DefaultOperatorImage = "cratis/showrunner-operator-image";

    /// <summary>
    /// The repository of the Understudy job image when none is given.
    /// </summary>
    public const string DefaultUnderstudyImage = "cratis/showrunner-understudy-image";

    /// <summary>
    /// The repository of the watchdog image when none is given.
    /// </summary>
    public const string DefaultWatchdogImage = "cratis/showrunner-watchdog-image";

    /// <summary>
    /// Initializes a new instance of the <see cref="StageManagerOperator"/> class.
    /// </summary>
    /// <param name="name">The Pulumi name of the component.</param>
    /// <param name="args">The inputs.</param>
    /// <param name="options">The component options.</param>
    public StageManagerOperator(string name, StageManagerOperatorArgs args, ComponentResourceOptions? options = null)
        : base(TypeToken, name, options)
    {
        ArgumentNullException.ThrowIfNull(args);
        Namespace = Output.Create(args.Namespace);
        OperatorImage = Output.Create(StageManagerOperatorValues.OperatorImage(args));
        UnderstudyImage = Output.Create(StageManagerOperatorValues.UnderstudyImage(args));
        WatchdogImage = Output.Create(StageManagerOperatorValues.WatchdogImage(args));

        var dependencies = new List<Resource>(args.DependsOn);
        if (args.Percona is { } percona)
        {
            // The Percona operator comes first: a Production that needs it is Blocked until it is there, so it should be there when the operator starts.
            PerconaRelease = new Release(
                $"{name}-percona",
                new ReleaseArgs
                {
                    Name = "psmdb-operator",
                    Chart = "psmdb-operator",
                    Version = percona.Version,
                    Namespace = percona.Namespace,
                    CreateNamespace = true,
                    RepositoryOpts = new RepositoryOptsArgs { Repo = percona.Repository },
                    SkipAwait = !args.WaitForOperator,
                    Values = ValuesOf(StageManagerOperatorValues.Merge(new Dictionary<string, object> { ["watchAllNamespaces"] = true }, percona.Values))
                },
                new CustomResourceOptions { Parent = this, Provider = args.Provider, DependsOn = dependencies });
            dependencies.Add(PerconaRelease);
        }

        var chartIsPath = args.Chart is not null && (args.Chart.StartsWith('.') || args.Chart.StartsWith('/'));
        Release = new Release(
            $"{name}-release",
            new ReleaseArgs
            {
                Name = args.ReleaseName,
                Chart = args.Chart ?? DefaultChart,
                Version = chartIsPath ? null : args.Version,
                Namespace = args.Namespace,
                CreateNamespace = args.CreateNamespace,
                SkipAwait = !args.WaitForOperator,
                Values = ValuesOf(StageManagerOperatorValues.Render(args))
            },
            new CustomResourceOptions { Parent = this, Provider = args.Provider, DependsOn = dependencies });

        RegisterOutputs(new Dictionary<string, object?>
        {
            ["namespace"] = Namespace,
            ["operatorImage"] = OperatorImage,
            ["understudyImage"] = UnderstudyImage,
            ["watchdogImage"] = WatchdogImage
        });
    }

    /// <summary>
    /// Gets the namespace the operator runs in.
    /// </summary>
    public Output<string> Namespace { get; }

    /// <summary>
    /// Gets the operator image with its tag.
    /// </summary>
    public Output<string> OperatorImage { get; }

    /// <summary>
    /// Gets the Understudy job image with its tag.
    /// </summary>
    public Output<string> UnderstudyImage { get; }

    /// <summary>
    /// Gets the watchdog image with the tag of the version, to pass as <c>WatchdogArgs.Image</c> so the watchdog and the operator never drift apart.
    /// </summary>
    public Output<string> WatchdogImage { get; }

    /// <summary>
    /// Gets the Helm release of the operator.
    /// </summary>
    public Release Release { get; }

    /// <summary>
    /// Gets the Helm release of the Percona operator, or null when none was asked for.
    /// </summary>
    public Release? PerconaRelease { get; }

    static InputMap<object> ValuesOf(IDictionary<string, object> values)
    {
        var map = new InputMap<object>();
        foreach (var (key, value) in values)
        {
            map[key] = value;
        }

        return map;
    }
}
