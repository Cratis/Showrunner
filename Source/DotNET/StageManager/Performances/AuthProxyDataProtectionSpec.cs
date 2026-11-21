// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// Where AuthProxy keeps its data protection key ring, so sign-in sessions survive a restart.
/// </summary>
public class AuthProxyDataProtectionSpec
{
    /// <summary>
    /// Gets or sets the PersistentVolumeClaim that holds the key ring.
    /// </summary>
    [Required]
    [Description("The PersistentVolumeClaim, in the namespace of the Performance, that holds the key ring. It is a ReadWriteOnce claim, so AuthProxy runs one replica and stops before it rolls out.")]
    public string ClaimName { get; set; } = string.Empty;
}
