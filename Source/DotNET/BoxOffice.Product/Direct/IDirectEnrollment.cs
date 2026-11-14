// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Product.Direct;

/// <summary>
/// The port to the thing that links the product to its Chronicle with an enrollment code. The product registers an
/// implementation that redeems the code through Chronicle's federation enrollment client API and keeps the issued
/// certificate where it keeps its credentials.
/// </summary>
public interface IDirectEnrollment
{
    /// <summary>
    /// Redeems an enrollment code and stores the credentials it yields. An implementation refuses an endpoint that is not
    /// on <see cref="DirectEnrollmentRequest.ExpectedHost"/> when one is given, and never reports or logs the code.
    /// </summary>
    /// <param name="request">The <see cref="DirectEnrollmentRequest"/>.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The <see cref="DirectRedemptionResult"/>.</returns>
    Task<DirectRedemptionResult> Redeem(DirectEnrollmentRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Makes an exchange over the link with the stored credentials, so it answers whether the link is established and
    /// healthy. It needs no code.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The <see cref="DirectLinkHealth"/>.</returns>
    Task<DirectLinkHealth> Verify(CancellationToken cancellationToken);
}
