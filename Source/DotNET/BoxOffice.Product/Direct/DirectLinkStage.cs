// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Product.Direct;

/// <summary>
/// How far the link to Chronicle has come.
/// </summary>
public enum DirectLinkStage
{
    /// <summary>
    /// No code has been redeemed; the page is open.
    /// </summary>
    NotConfigured = 0,

    /// <summary>
    /// A code was redeemed and the credentials are stored, but the first exchange has not succeeded; the page stays open.
    /// </summary>
    Redeemed = 1,

    /// <summary>
    /// The first exchange succeeded; the page is closed for good.
    /// </summary>
    Closed = 2
}
