// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// Where an application is reached from outside the cluster.
/// </summary>
public static class PublicEndpoint
{
    /// <summary>
    /// Gets the public URL of a Performance: the first ingress host over HTTPS.
    /// </summary>
    /// <param name="spec">The spec.</param>
    /// <returns>The URL without a trailing slash, or an empty string when the Performance has no public host.</returns>
    public static string AddressOf(PerformanceSpec spec) =>
        spec.Ingress.Enabled && spec.Ingress.Hosts.Count > 0 ? $"https://{spec.Ingress.Hosts[0]}" : string.Empty;
}
