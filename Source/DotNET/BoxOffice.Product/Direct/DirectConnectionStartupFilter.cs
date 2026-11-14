// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice.Product.Direct;

/// <summary>
/// Puts the setup page in front of the product's pipeline when it is enabled. When it is not, nothing is added, so the
/// path does not exist and answers whatever the application answers for an unknown path.
/// </summary>
/// <param name="options">The <see cref="BoxOfficeProductOptions"/>.</param>
public class DirectConnectionStartupFilter(IOptions<BoxOfficeProductOptions> options) : IStartupFilter
{
    /// <inheritdoc/>
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
    {
        if (options.Value.Direct.Enabled)
        {
            _ = app.ApplicationServices.GetService(typeof(IDirectEnrollment))
                ?? throw new InvalidOperationException("The Direct connection page is enabled, so an IDirectEnrollment that redeems the code through Chronicle must be registered.");
            app.UseMiddleware<DirectConnectionMiddleware>();
        }

        next(app);
    };
}
