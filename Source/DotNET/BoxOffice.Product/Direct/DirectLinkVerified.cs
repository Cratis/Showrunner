// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Product.Direct;

/// <summary>
/// Emitted when the first exchange over the link to Chronicle succeeded.
/// </summary>
/// <param name="Endpoint">The endpoint of the link.</param>
[EventType]
public record DirectLinkVerified(string Endpoint);
