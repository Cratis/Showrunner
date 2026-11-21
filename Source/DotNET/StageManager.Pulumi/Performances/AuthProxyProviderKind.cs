// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Performances;

/// <summary>
/// The kind of identity provider an application signs users in with.
/// </summary>
public enum AuthProxyProviderKind
{
    /// <summary>
    /// Microsoft Entra ID.
    /// </summary>
    EntraId = 0,

    /// <summary>
    /// GitHub.
    /// </summary>
    GitHub = 1,

    /// <summary>
    /// Google.
    /// </summary>
    Google = 2,

    /// <summary>
    /// Apple.
    /// </summary>
    Apple = 3,

    /// <summary>
    /// Any OpenID Connect provider.
    /// </summary>
    OpenId = 4
}
