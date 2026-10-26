// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Cratis.Showrunner.Usher.Access.OnBehalf;

/// <summary>
/// Puts <see cref="OnBehalfForwards"/> first in a web application's pipeline, ahead of authentication, so no
/// unverified on-behalf forward reaches anything that believes the forwarded identity.
/// </summary>
public class OnBehalfStartupFilter : IStartupFilter
{
    /// <inheritdoc/>
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
    {
        app.UseMiddleware<OnBehalfForwards>();
        next(app);
    };
}
