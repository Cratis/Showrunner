// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// Turns what an <see cref="ApplicationSpec"/> says about the pod of a rendered application into the shape of its workload.
/// </summary>
public static class ApplicationPod
{
    /// <summary>
    /// The prefix of the name a volume of the application has in the pod, so it never meets a volume Stage Manager mounts itself.
    /// </summary>
    public const string VolumePrefix = "app-";

    /// <summary>
    /// Creates the shape of the pod.
    /// </summary>
    /// <param name="application">The application.</param>
    /// <returns>The shape.</returns>
    public static PodShape Shape(ApplicationSpec application) => new(
        EmptyToNull(application.Probes.LivenessPath),
        EmptyToNull(application.Probes.StartupPath),
        [.. application.ImagePullSecrets],
        [.. application.Volumes.Select(Volume)],
        [.. application.VolumeMounts.Select(Mount)],
        [.. application.InitContainers.Select(container => InitContainer(application, container))],
        application.NodeSelector.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal));

    /// <summary>
    /// Creates the resource requirements of the application container.
    /// </summary>
    /// <param name="application">The application.</param>
    /// <returns>The requirements, or null when none are set.</returns>
    public static V1ResourceRequirements? Resources(ApplicationSpec application) => application.Resources is { } resources && (resources.Requests.Count > 0 || resources.Limits.Count > 0)
        ? new()
        {
            Requests = Quantities(resources.Requests),
            Limits = Quantities(resources.Limits)
        }
        : null;

    static string? EmptyToNull(string value) => string.IsNullOrWhiteSpace(value) ? null : value;

    static Dictionary<string, ResourceQuantity>? Quantities(IDictionary<string, string> given) =>
        given.Count == 0 ? null : given.ToDictionary(pair => pair.Key, pair => new ResourceQuantity(pair.Value), StringComparer.Ordinal);

    static V1Volume Volume(ApplicationVolume volume) => string.Equals(volume.Kind, ApplicationVolume.PersistentVolumeClaim, StringComparison.Ordinal)
        ? new() { Name = $"{VolumePrefix}{volume.Name}", PersistentVolumeClaim = new() { ClaimName = volume.ClaimName } }
        : new() { Name = $"{VolumePrefix}{volume.Name}", EmptyDir = new() };

    static V1VolumeMount Mount(ApplicationVolumeMount mount) => new() { Name = $"{VolumePrefix}{mount.Volume}", MountPath = mount.Path, ReadOnlyProperty = mount.ReadOnly };

    static V1Container InitContainer(ApplicationSpec application, ApplicationInitContainer container) => new()
    {
        Name = container.Name,
        Image = string.IsNullOrWhiteSpace(container.Image) ? application.Image : container.Image,
        Command = container.Command.Count > 0 ? [.. container.Command] : null,
        Args = container.Args.Count > 0 ? [.. container.Args] : null,
        Env = [.. container.Environment.OrderBy(pair => pair.Key, StringComparer.Ordinal).Select(pair => new V1EnvVar { Name = pair.Key, Value = pair.Value })],
        VolumeMounts = [.. container.VolumeMounts.Select(Mount)],
        SecurityContext = new() { AllowPrivilegeEscalation = false, Capabilities = new() { Drop = ["ALL"] } }
    };
}
