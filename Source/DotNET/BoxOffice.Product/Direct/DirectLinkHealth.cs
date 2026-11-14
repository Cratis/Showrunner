// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Product.Direct;

/// <summary>
/// How the link to Chronicle is, as an exchange over it shows.
/// </summary>
public enum DirectLinkHealth
{
    /// <summary>
    /// The exchange succeeded.
    /// </summary>
    Healthy = 0,

    /// <summary>
    /// The exchange failed in a way a retry with the stored credentials may fix.
    /// </summary>
    Unreachable = 1,

    /// <summary>
    /// The stored credentials can never work again, so only a new enrollment can restore the link.
    /// </summary>
    Unrecoverable = 2
}
