// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// What self-service sign-up does in an application with onboarding.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SignUpMode
{
    /// <summary>
    /// People can register and create a tenant.
    /// </summary>
    CreateTenant = 0,

    /// <summary>
    /// People can only join an existing tenant by invitation.
    /// </summary>
    JoinTenant = 1
}
