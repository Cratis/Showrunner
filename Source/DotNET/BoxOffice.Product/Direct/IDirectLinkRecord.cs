// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Product.Direct;

/// <summary>
/// Records the facts of the link to Chronicle durably, so a closed page stays closed across restarts.
/// </summary>
public interface IDirectLinkRecord
{
    /// <summary>
    /// Reads the recorded state.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The <see cref="DirectLinkState"/>.</returns>
    Task<DirectLinkState> Read(CancellationToken cancellationToken);

    /// <summary>
    /// Records that a code was redeemed and its credentials stored.
    /// </summary>
    /// <param name="endpoint">The endpoint the code carried.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>A <see cref="Task"/> for continuation.</returns>
    Task Redeemed(string endpoint, CancellationToken cancellationToken);

    /// <summary>
    /// Records that the first exchange succeeded, which closes the page.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>A <see cref="Task"/> for continuation.</returns>
    Task Verified(CancellationToken cancellationToken);

    /// <summary>
    /// Records that the page opened again to enroll anew.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>A <see cref="Task"/> for continuation.</returns>
    Task Reopened(CancellationToken cancellationToken);
}
