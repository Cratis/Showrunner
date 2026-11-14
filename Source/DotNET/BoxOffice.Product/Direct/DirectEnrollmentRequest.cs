// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Product.Direct;

/// <summary>
/// A request to redeem an enrollment code.
/// </summary>
/// <param name="Code">The enrollment code, which carries the endpoint, the fingerprint of the certificate authority and a secret. Never log it.</param>
/// <param name="Endpoint">A manually typed endpoint, already checked against the pinned host, or <see langword="null"/> to use the one the code carries.</param>
/// <param name="ExpectedHost">The host the deployment pins, or empty when it pins none. An endpoint the code carries must be on it too.</param>
public record DirectEnrollmentRequest(string Code, string? Endpoint, string ExpectedHost);
