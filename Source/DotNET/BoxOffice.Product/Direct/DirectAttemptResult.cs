// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Product.Direct;

/// <summary>
/// How an attempt to link the product to Chronicle went.
/// </summary>
public enum DirectAttemptResult
{
    /// <summary>
    /// The code was redeemed, or its credentials were already stored, and the first exchange succeeded.
    /// </summary>
    Connected = 0,

    /// <summary>
    /// The link is already closed; there is nothing to do.
    /// </summary>
    Closed = 1,

    /// <summary>
    /// No usable code was given.
    /// </summary>
    NoCode = 2,

    /// <summary>
    /// The code was not accepted.
    /// </summary>
    CodeRefused = 3,

    /// <summary>
    /// The endpoint is not one the deployment allows.
    /// </summary>
    EndpointRefused = 4,

    /// <summary>
    /// The endpoint could not be reached to redeem the code.
    /// </summary>
    Unreachable = 5,

    /// <summary>
    /// The code was redeemed but the first exchange failed; a retry needs no new code.
    /// </summary>
    ConnectionFailed = 6,

    /// <summary>
    /// Too many attempts were made in the window.
    /// </summary>
    TooManyAttempts = 7
}
