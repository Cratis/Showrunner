// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Product.Direct;

/// <summary>
/// Emitted when an enrollment code was redeemed and its credentials stored. It never carries the code.
/// </summary>
/// <param name="Endpoint">The endpoint the code carried.</param>
[EventType]
public record DirectLinkRedeemed(string Endpoint);
