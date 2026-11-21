// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Performances;

/// <summary>
/// A destination the application's pods may connect to, on the ports named.
/// </summary>
/// <remarks>
/// Naming any rule isolates the pods for egress: they may then reach DNS, their own namespace and the destinations named, and
/// nothing else. A rule with neither a <see cref="Cidr"/> nor a <see cref="Namespace"/> opens the public internet: every
/// address except the private ranges 10.0.0.0/8, 172.16.0.0/12 and 192.168.0.0/16.
/// </remarks>
public sealed class EgressRuleArgs
{
    /// <summary>
    /// Gets the address range in CIDR notation, such as <c>203.0.113.0/24</c>.
    /// </summary>
    public string? Cidr { get; init; }

    /// <summary>
    /// Gets the namespace whose pods the application may reach.
    /// </summary>
    public string? Namespace { get; init; }

    /// <summary>
    /// Gets the TCP ports the application may connect to. At least one.
    /// </summary>
    public IList<int> Ports { get; init; } = [];

    /// <summary>
    /// Builds the spec of the rule.
    /// </summary>
    /// <returns>The spec.</returns>
    internal Dictionary<string, object> ToSpec() =>
        SpecExtensions.NewSpec().Put("cidr", Cidr).Put("namespace", Namespace).PutList("ports", Ports);
}
