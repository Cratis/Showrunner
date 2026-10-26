// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Http;

namespace Cratis.Showrunner.Usher.Access.OnBehalf;

/// <summary>
/// Represents the default <see cref="IOnBehalfForward"/>: the forward <see cref="OnBehalfForwards"/> verified and
/// decided for the current HTTP request.
/// </summary>
/// <param name="http">The <see cref="IHttpContextAccessor"/>.</param>
public class OnBehalfForward(IHttpContextAccessor http) : IOnBehalfForward
{
    /// <inheritdoc/>
    public VerifiedForward? Current =>
        http.HttpContext?.Items.TryGetValue(OnBehalfForwards.VerifiedForward, out var forward) == true ? forward as VerifiedForward : null;
}
