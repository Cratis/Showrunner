// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.BoxOffice.Product.Direct;

/// <summary>
/// The recorded state of the link to Chronicle.
/// </summary>
/// <param name="Stage">How far the link has come.</param>
/// <param name="Endpoint">The endpoint taken from the code, empty until a code is redeemed. Box Office's operator store maps to it.</param>
public record DirectLinkState(DirectLinkStage Stage, string Endpoint);
