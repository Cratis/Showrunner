// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// An explicit connection to a Chronicle.
/// </summary>
public class ChronicleConnection
{
    /// <summary>
    /// Gets or sets the Secret holding the connection string. The Secret is mounted into the workloads; it is never copied.
    /// </summary>
    [Required]
    [Description("The Secret holding the Chronicle connection string. It is mounted into the workloads, never copied.")]
    public SecretKeyReference ConnectionSecretRef { get; set; } = new();
}
