// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Performances;

/// <summary>
/// What self-service sign-up does.
/// </summary>
public enum SignUpMode
{
    /// <summary>
    /// A person who signs up creates a tenant.
    /// </summary>
    CreateTenant = 0,

    /// <summary>
    /// A person who signs up joins an existing tenant by invitation.
    /// </summary>
    JoinTenant = 1
}
