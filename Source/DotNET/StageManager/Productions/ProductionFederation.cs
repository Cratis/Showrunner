// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// How the Chronicle of a <see cref="Production"/> takes part in federation: serving a listener, acting as the authority customers enroll with, or dialing out to an authority.
/// </summary>
/// <remarks>
/// A Production that only dials out is the sending side of a one-way link: it opens no listener and no port, and pushes its public events to the authority it enrolled with.
/// </remarks>
public class ProductionFederation
{
    /// <summary>
    /// The port of the federation listener when none is given.
    /// </summary>
    public const int DefaultPort = 35100;

    /// <summary>
    /// Gets or sets whether Chronicle opens the federation listener and runs consumer bindings.
    /// </summary>
    [Description("Opens the federation listener and runs consumer bindings (Federation:Enabled). It presents the certificate of Chronicle, so the host of hosted.publicEndpoint is added to it. Off by default.")]
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the port of the federation listener.
    /// </summary>
    [Description("The port of the federation listener, and of the federation Service port. Defaults to 35100. Must differ from the other Chronicle ports.")]
    public int Port { get; set; } = DefaultPort;

    /// <summary>
    /// Gets or sets the hosted federation authority settings.
    /// </summary>
    [Description("Makes Chronicle the certificate authority for the links of its customers (Federation:Hosted). Needs enabled.")]
    public ProductionHostedFederation Hosted { get; set; } = new();

    /// <summary>
    /// Gets or sets the dial-out settings.
    /// </summary>
    [Description("Makes Chronicle dial an authority it enrolled with (Federation:Dial). It opens no listener and needs no open port.")]
    public ProductionFederationDial Dial { get; set; } = new();
}
