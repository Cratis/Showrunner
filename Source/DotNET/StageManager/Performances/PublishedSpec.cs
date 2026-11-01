// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// Runs the application as a published Stage application: the generic Stage runtime image in published mode with an application package mounted into it.
/// </summary>
public class PublishedSpec
{
    /// <summary>
    /// Gets or sets the application version this package is.
    /// </summary>
    [Required]
    [Description("The application version of the package, for example 1.4.0. A change of version, image or package rolls out; a rollout that is not ready in time rolls back.")]
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the immutable ConfigMap holding the package archive.
    /// </summary>
    [Required]
    [Description("An immutable ConfigMap whose binaryData key (package.tar.gz unless key is set) holds the exported application package archive.")]
    public ConfigMapKeyReference PackageConfigMapRef { get; set; } = new();

    /// <summary>
    /// Gets or sets the Secret holding the MongoDB connection string for the kernel inside Stage.
    /// </summary>
    [Required]
    [Description("The Secret holding the MongoDB connection string of the in-process Chronicle kernel. Use a database of its own for each application. It is mounted as a file and never copied.")]
    public SecretKeyReference StorageConnectionSecretRef { get; set; } = new();

    /// <summary>
    /// Gets or sets the addresses Stage trusts to forward the signed-in principal.
    /// </summary>
    [Description("IP addresses or CIDR networks AuthProxy connects from. Stage believes the principal headers only from these; use the pod network of the cluster. A network policy lets only AuthProxy reach the application.")]
    public IList<string> TrustedProxies { get; set; } = [];

    /// <summary>
    /// Gets or sets how long a rollout may take before it is rolled back.
    /// </summary>
    [Description("How many seconds a new version may take to become ready before the rollout is rolled back to the previous version.")]
    public int RolloutTimeoutSeconds { get; set; } = 300;

    /// <summary>
    /// Gets or sets the resources of the Stage container.
    /// </summary>
    [Description("The resources of the Stage container. Defaults to requests of 250m CPU and 512Mi memory and limits of 1 CPU and 1Gi memory.")]
    public ResourcesSpec Resources { get; set; } = new();

    /// <summary>
    /// Gets or sets the Notes backend settings. Omit when the application does not host Notes.
    /// </summary>
    [Description("Hosts the Notes backend and sends notes over HTTP to a collector. Omit for an application without Notes.")]
    public PublishedNotesSpec? Notes { get; set; }
}
