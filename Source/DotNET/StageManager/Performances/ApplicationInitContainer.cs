// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// A container that runs to completion before the application starts, such as one that prepares a shared volume.
/// </summary>
public class ApplicationInitContainer
{
    /// <summary>
    /// Gets or sets the name of the container.
    /// </summary>
    [Description("The name of the init container, as a DNS label.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the image of the container.
    /// </summary>
    [Description("The image of the init container. Defaults to the image of the application.")]
    public string Image { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the command of the container.
    /// </summary>
    [Description("The command that replaces the entrypoint of the image.")]
    public IList<string> Command { get; set; } = [];

    /// <summary>
    /// Gets or sets the arguments of the command.
    /// </summary>
    [Description("The arguments of the command.")]
    public IList<string> Args { get; set; } = [];

    /// <summary>
    /// Gets or sets the plain environment variables of the container.
    /// </summary>
    [Description("Plain environment variables. Never put a secret here; the container gets no secret from the Performance.")]
    public IDictionary<string, string> Environment { get; set; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>
    /// Gets or sets the volumes the container mounts.
    /// </summary>
    [Description("The volumes of the application this container mounts.")]
    public IList<ApplicationVolumeMount> VolumeMounts { get; set; } = [];
}
