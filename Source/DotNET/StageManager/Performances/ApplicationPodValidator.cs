// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net;
using System.Text.RegularExpressions;

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// Finds what is wrong with what an <see cref="ApplicationSpec"/> says about its pod: strategy, probes, volumes, init containers, resources and egress.
/// </summary>
internal static partial class ApplicationPodValidator
{
    static readonly string[] _strategies = [string.Empty, ApplicationSpec.RollingUpdate, ApplicationSpec.Recreate];
    static readonly string[] _volumeKinds = [ApplicationVolume.EmptyDir, ApplicationVolume.PersistentVolumeClaim];

    [GeneratedRegex("^[a-z0-9]([-a-z0-9]{0,61}[a-z0-9])?$", RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 1000)]
    private static partial Regex DnsLabel { get; }

    /// <summary>
    /// Validates the pod of the application.
    /// </summary>
    /// <param name="application">The application.</param>
    /// <param name="problems">The problems to add to.</param>
    internal static void Validate(ApplicationSpec application, List<string> problems)
    {
        if (!_strategies.Contains(application.Strategy, StringComparer.Ordinal))
        {
            problems.Add($"application.strategy must be RollingUpdate or Recreate, not '{application.Strategy}'.");
        }

        if (application.Published is not null)
        {
            ValidateNotPublished(application, problems);
        }
        else
        {
            ValidateProbes(application.Probes, problems);
            ValidateVolumes(application, problems);
            ValidateInitContainers(application, problems);
            ValidateResources(application.Resources, problems);
        }

        ValidateEgress(application.Egress, problems);
    }

    static void ValidateNotPublished(ApplicationSpec application, List<string> problems)
    {
        var shaped = new Dictionary<string, bool>(StringComparer.Ordinal)
        {
            ["probes"] = !string.IsNullOrWhiteSpace(application.Probes.ReadinessPath) || !string.IsNullOrWhiteSpace(application.Probes.LivenessPath) || !string.IsNullOrWhiteSpace(application.Probes.StartupPath),
            ["resources"] = application.Resources is not null,
            ["volumes"] = application.Volumes.Count > 0 || application.VolumeMounts.Count > 0,
            ["initContainers"] = application.InitContainers.Count > 0
        };
        foreach (var (field, set) in shaped.Where(pair => pair.Value))
        {
            problems.Add($"application.{field} does not apply to a published application, whose pod Stage Manager shapes itself.");
        }

        if (!string.IsNullOrEmpty(application.Strategy))
        {
            problems.Add("application.strategy does not apply to a published application, which always stops the old pod before it starts the new one.");
        }
    }

    static void ValidateProbes(ApplicationProbes probes, List<string> problems)
    {
        foreach (var (field, path) in new[] { ("readinessPath", probes.ReadinessPath), ("livenessPath", probes.LivenessPath), ("startupPath", probes.StartupPath) })
        {
            if (!string.IsNullOrEmpty(path) && !path.StartsWith('/'))
            {
                problems.Add($"application.probes.{field} must start with /, not '{path}'.");
            }
        }
    }

    static void ValidateVolumes(ApplicationSpec application, List<string> problems)
    {
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var volume in application.Volumes)
        {
            if (!DnsLabel.IsMatch(volume.Name))
            {
                problems.Add($"application.volumes name '{volume.Name}' must be a DNS label: lower case letters, digits and -.");
            }
            else if (!names.Add(volume.Name))
            {
                problems.Add($"application.volumes names '{volume.Name}' twice.");
            }

            if (!_volumeKinds.Contains(volume.Kind, StringComparer.Ordinal))
            {
                problems.Add($"application.volumes '{volume.Name}' has kind '{volume.Kind}'; use EmptyDir or PersistentVolumeClaim.");
            }
            else if (volume.Kind == ApplicationVolume.PersistentVolumeClaim && string.IsNullOrWhiteSpace(volume.ClaimName))
            {
                problems.Add($"application.volumes '{volume.Name}' is a PersistentVolumeClaim and needs claimName.");
            }
        }

        ValidateMounts("application.volumeMounts", application.VolumeMounts, names, problems);
    }

    static void ValidateMounts(string field, IEnumerable<ApplicationVolumeMount> mounts, HashSet<string> volumes, List<string> problems)
    {
        foreach (var mount in mounts)
        {
            if (!volumes.Contains(mount.Volume))
            {
                problems.Add($"{field} mounts the volume '{mount.Volume}', which application.volumes does not declare.");
            }

            if (!mount.Path.StartsWith('/'))
            {
                problems.Add($"{field} path '{mount.Path}' must be absolute.");
            }
        }
    }

    static void ValidateInitContainers(ApplicationSpec application, List<string> problems)
    {
        var volumes = application.Volumes.Select(volume => volume.Name).ToHashSet(StringComparer.Ordinal);
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var container in application.InitContainers)
        {
            if (!DnsLabel.IsMatch(container.Name))
            {
                problems.Add($"application.initContainers name '{container.Name}' must be a DNS label: lower case letters, digits and -.");
            }
            else if (!names.Add(container.Name))
            {
                problems.Add($"application.initContainers names '{container.Name}' twice.");
            }

            ValidateMounts($"application.initContainers '{container.Name}' volumeMounts", container.VolumeMounts, volumes, problems);
        }
    }

    static void ValidateResources(ResourcesSpec? resources, List<string> problems)
    {
        if (resources is null)
        {
            return;
        }

        foreach (var (kind, quantities) in new[] { ("requests", resources.Requests), ("limits", resources.Limits) })
        {
            foreach (var (name, quantity) in quantities)
            {
                if (!Quantity(quantity))
                {
                    problems.Add($"application.resources.{kind}.{name} '{quantity}' is not a Kubernetes quantity such as 250m or 512Mi.");
                }
            }
        }
    }

    static bool Quantity(string value)
    {
        try
        {
            _ = new ResourceQuantity(value);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    static void ValidateEgress(IList<ApplicationEgressRule> rules, List<string> problems)
    {
        for (var index = 0; index < rules.Count; index++)
        {
            var rule = rules[index];
            var field = $"application.egress[{index}]";
            if (rule.Ports.Count == 0 || rule.Ports.Any(port => port is < 1 or > 65535))
            {
                problems.Add($"{field}.ports needs at least one port from 1 to 65535.");
            }

            if (!string.IsNullOrWhiteSpace(rule.Cidr) && !string.IsNullOrWhiteSpace(rule.Namespace))
            {
                problems.Add($"{field} names both cidr and namespace; name one of them, or neither for the public internet.");
            }
            else if (!string.IsNullOrWhiteSpace(rule.Cidr) && !IPNetwork.TryParse(rule.Cidr, out _))
            {
                problems.Add($"{field}.cidr '{rule.Cidr}' is not an address range in CIDR notation.");
            }
        }
    }
}
