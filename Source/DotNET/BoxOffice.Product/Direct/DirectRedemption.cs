// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Product.Direct;

/// <summary>
/// How redeeming an enrollment code went.
/// </summary>
public enum DirectRedemption
{
    /// <summary>
    /// The code was redeemed and the credentials are stored.
    /// </summary>
    Redeemed = 0,

    /// <summary>
    /// The code was not accepted: it is wrong, expired, used or malformed.
    /// </summary>
    Refused = 1,

    /// <summary>
    /// The endpoint the code names could not be reached.
    /// </summary>
    Unreachable = 2,

    /// <summary>
    /// The endpoint is not on the host the deployment pins.
    /// </summary>
    EndpointNotAllowed = 3
}
