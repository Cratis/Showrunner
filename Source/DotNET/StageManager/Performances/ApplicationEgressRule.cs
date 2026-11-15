// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// A destination the application's pods may connect to, on the ports named.
/// </summary>
/// <remarks>
/// Naming any rule isolates the pods for egress: they may then reach DNS, their own namespace and the destinations named, and nothing else.
/// A rule with neither a cidr nor a namespace opens the public internet: every address except the private ranges 10.0.0.0/8, 172.16.0.0/12 and 192.168.0.0/16.
/// </remarks>
public class ApplicationEgressRule
{
    /// <summary>
    /// Gets or sets the address range.
    /// </summary>
    [Description("An address range in CIDR notation, such as 203.0.113.0/24. Empty with no namespace means every public address.")]
    public string Cidr { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the namespace.
    /// </summary>
    [Description("A namespace whose pods the application may reach. Use it for a Chronicle or database in another namespace.")]
    public string Namespace { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the TCP ports.
    /// </summary>
    [Description("The TCP ports the application may connect to. At least one.")]
    public IList<int> Ports { get; set; } = [];
}
