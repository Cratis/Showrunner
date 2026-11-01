// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// CPU and memory requests and limits of a container, as Kubernetes quantities.
/// </summary>
public class ResourcesSpec
{
    /// <summary>
    /// Gets or sets the requests by resource name, for example <c>cpu</c> and <c>memory</c>.
    /// </summary>
    [Description("Requests by resource name, for example cpu: 250m and memory: 512Mi.")]
    public IDictionary<string, string> Requests { get; set; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>
    /// Gets or sets the limits by resource name.
    /// </summary>
    [Description("Limits by resource name, for example cpu: '1' and memory: 1Gi.")]
    public IDictionary<string, string> Limits { get; set; } = new Dictionary<string, string>(StringComparer.Ordinal);
}
