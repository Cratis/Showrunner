// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Product.Direct;

/// <summary>
/// Emitted when the setup page opened again because the link could never work again and re-enrollment is allowed.
/// </summary>
/// <param name="Endpoint">The endpoint of the link that was given up.</param>
[EventType]
public record DirectLinkReopened(string Endpoint);
