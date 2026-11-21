// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Pulumi;

namespace Cratis.Showrunner.StageManager.Pulumi.Performances;

/// <summary>
/// A container that runs to completion before the application starts, such as one that prepares a shared volume.
/// </summary>
public sealed class InitContainerArgs
{
    /// <summary>
    /// Gets the name of the init container, as a DNS label.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the image of the init container, or null to use the image of the application.
    /// </summary>
    public Input<string>? Image { get; init; }

    /// <summary>
    /// Gets the command that replaces the entrypoint of the image.
    /// </summary>
    public IList<string> Command { get; init; } = [];

    /// <summary>
    /// Gets the arguments of the command.
    /// </summary>
    public IList<string> Args { get; init; } = [];

    /// <summary>
    /// Gets the plain environment variables. Never put a secret here; the container gets no secret from the Performance.
    /// </summary>
    public IDictionary<string, string> Environment { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>
    /// Gets the volumes of the application this container mounts.
    /// </summary>
    public IList<VolumeMountArgs> VolumeMounts { get; init; } = [];

    /// <summary>
    /// Builds the spec of the init container.
    /// </summary>
    /// <returns>The spec.</returns>
    internal Dictionary<string, object> ToSpec() =>
        SpecExtensions.NewSpec()
            .Put("name", Name)
            .Put("image", Image)
            .PutList("command", Command)
            .PutList("args", Args)
            .PutMap("environment", Environment.Select(pair => new KeyValuePair<string, object>(pair.Key, pair.Value)))
            .PutList("volumeMounts", VolumeMounts.Select(mount => mount.ToSpec()));
}
