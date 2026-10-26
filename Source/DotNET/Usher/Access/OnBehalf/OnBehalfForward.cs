// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Http;

namespace Cratis.Showrunner.Usher.Access.OnBehalf;

/// <summary>
/// Represents the default <see cref="IOnBehalfForward"/>: the assertion <see cref="OnBehalfForwards"/> verified for
/// the current HTTP request.
/// </summary>
/// <param name="http">The <see cref="IHttpContextAccessor"/>.</param>
public class OnBehalfForward(IHttpContextAccessor http) : IOnBehalfForward
{
    /// <inheritdoc/>
    public OnBehalfAssertion? Current =>
        http.HttpContext?.Items.TryGetValue(OnBehalfForwards.VerifiedAssertion, out var assertion) == true ? assertion as OnBehalfAssertion : null;
}
