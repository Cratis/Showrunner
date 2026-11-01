// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// What a <see cref="Performance"/> runs and how.
/// </summary>
public class PerformanceSpec
{
    /// <summary>
    /// Gets or sets the Production whose Chronicle the application uses.
    /// </summary>
    [Description("The Production whose Chronicle the application uses. Give this or chronicle.connectionSecretRef.")]
    public ProductionReference? ProductionRef { get; set; }

    /// <summary>
    /// Gets or sets an explicit Chronicle connection, used when no Production is referenced.
    /// </summary>
    [Description("An explicit Chronicle connection. Takes precedence over productionRef.")]
    public ChronicleConnection? Chronicle { get; set; }

    /// <summary>
    /// Gets or sets the application to run.
    /// </summary>
    [Required]
    [Description("The application to run.")]
    public ApplicationSpec Application { get; set; } = new();

    /// <summary>
    /// Gets or sets the Cratis environment the application runs.
    /// </summary>
    [Required]
    [Description("The Cratis environment the application runs.")]
    public EnvironmentSpec Environment { get; set; } = new();

    /// <summary>
    /// Gets or sets the AuthProxy that fronts the application.
    /// </summary>
    [Description("The AuthProxy that fronts the application.")]
    public AuthProxySpec AuthProxy { get; set; } = new();

    /// <summary>
    /// Gets or sets the onboarding service, one Ante for this application.
    /// </summary>
    [Description("The onboarding service: exactly one Ante for this application.")]
    public OnboardingSpec Onboarding { get; set; } = new();

    /// <summary>
    /// Gets or sets how the application is reached from outside the cluster.
    /// </summary>
    [Description("How the application is reached from outside the cluster. AuthProxy is the only public entry point.")]
    public IngressSpec Ingress { get; set; } = new();

    /// <summary>
    /// Gets or sets how the generated signing keys are rotated.
    /// </summary>
    [Description("How the generated signing keys are rotated.")]
    public SigningKeysSpec SigningKeys { get; set; } = new();

    /// <summary>
    /// Gets or sets how long the Performance runs. Omit to run permanently.
    /// </summary>
    [Description("How long the Performance runs: permanently, or until a given time. Omit to run permanently.")]
    public LifetimeSpec? Lifetime { get; set; }
}
