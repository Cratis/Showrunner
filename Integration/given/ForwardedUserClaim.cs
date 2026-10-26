// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Usher.Integration.given;

/// <summary>
/// A claim of a <see cref="ForwardedUser"/>.
/// </summary>
/// <param name="Typ">The type.</param>
/// <param name="Val">The value.</param>
public record ForwardedUserClaim(string Typ, string Val);
