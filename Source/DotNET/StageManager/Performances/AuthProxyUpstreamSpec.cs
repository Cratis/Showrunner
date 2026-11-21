// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// An existing Service AuthProxy sits in front of. A <see cref="Performance"/> that names an upstream is a gateway: it runs AuthProxy only, with no application, no Ante and no Chronicle.
/// </summary>
public class AuthProxyUpstreamSpec
{
    /// <summary>
    /// Gets or sets the address AuthProxy forwards to.
    /// </summary>
    [Required]
    [Description("The address AuthProxy forwards everything it lets through to, for example http://grafana.infrastructure.svc.cluster.local:3000/.")]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the upstream is not a Cratis service.
    /// </summary>
    [Description("Whether the upstream is not a Cratis service, such as a dashboard or Grafana: it has no /.cratis/me, so AuthProxy only decides who may get in and does not ask it who is calling.")]
    public bool ThirdParty { get; set; } = true;
}
