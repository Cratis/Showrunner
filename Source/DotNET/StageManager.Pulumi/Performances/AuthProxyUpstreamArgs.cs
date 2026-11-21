// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Performances;

/// <summary>
/// An existing Service AuthProxy sits in front of. A Performance that names one is a gateway: it runs AuthProxy only, with no application, no Ante and no Chronicle.
/// </summary>
public sealed class AuthProxyUpstreamArgs
{
    /// <summary>
    /// Gets the address AuthProxy forwards to, for example <c>http://grafana.infrastructure.svc.cluster.local:3000/</c>.
    /// </summary>
    public required string Url { get; init; }

    /// <summary>
    /// Gets whether the upstream is not a Cratis service, such as a dashboard or Grafana. True by default.
    /// </summary>
    public bool ThirdParty { get; init; } = true;

    /// <summary>
    /// Builds the spec of the upstream.
    /// </summary>
    /// <returns>The spec.</returns>
    internal Dictionary<string, object> ToSpec() =>
        SpecExtensions.NewSpec()
            .Put("url", Url)
            .Put("thirdParty", ThirdParty ? null : false);
}
