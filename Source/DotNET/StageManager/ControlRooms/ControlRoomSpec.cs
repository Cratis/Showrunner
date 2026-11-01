// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.ControlRooms;

/// <summary>
/// What a <see cref="ControlRoom"/> asks for. Schema only: the reconciliation that acts on it is a separate piece of work.
/// </summary>
public class ControlRoomSpec
{
    /// <summary>
    /// Gets or sets the address of the control plane to connect out to.
    /// </summary>
    [Required]
    [Description("The address of the control plane to connect out to.")]
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the Secret holding the client certificate for the mutually authenticated connection.
    /// </summary>
    [Description("The name of the Secret holding the client certificate.")]
    public string CertificateSecretName { get; set; } = string.Empty;
}
