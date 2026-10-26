// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Cratis.Showrunner.Usher.Access.OnBehalf;

/// <summary>
/// Puts <see cref="OnBehalfForwards"/> first in a web application's pipeline, so no on-behalf forward Arc did not
/// accept reaches anything in the application, and every one it did carries the decision on its session.
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
