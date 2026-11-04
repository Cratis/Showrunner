// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice.Product.SignIns;

/// <summary>
/// Puts the sign-in callback in front of the product's pipeline, so a product that adds Box Office's product part
/// receives it without mapping an endpoint.
/// </summary>
/// <param name="options">The <see cref="BoxOfficeProductOptions"/>, which say whether the product receives the callback.</param>
public class SignInCallbackStartupFilter(IOptions<BoxOfficeProductOptions> options) : IStartupFilter
{
    /// <inheritdoc/>
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
    {
        if (options.Value.ReportSignIns)
        {
            app.UseMiddleware<SignInCallbackMiddleware>();
        }

        next(app);
    };
}
