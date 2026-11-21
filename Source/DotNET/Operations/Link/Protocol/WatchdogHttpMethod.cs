// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Link.Protocol;

/// <summary>
/// An HTTP method a check or remedy is pinned to.
/// </summary>
public enum WatchdogHttpMethod
{
    /// <summary>
    /// <c>GET</c> - checks only.
    /// </summary>
    Get = 0,

    /// <summary>
    /// <c>HEAD</c> - checks only.
    /// </summary>
    Head = 1,

    /// <summary>
    /// <c>POST</c> - remedies only.
    /// </summary>
    Post = 2,

    /// <summary>
    /// <c>PUT</c> - remedies only.
    /// </summary>
    Put = 3,
}
