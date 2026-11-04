// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice.Product.SignIns;

/// <summary>
/// Answers AuthProxy's sign-in callback on its path, and leaves every other request alone. Its path is
/// <see cref="BoxOfficeProductOptions.SignInPath"/>.
/// </summary>
/// <param name="next">The next <see cref="RequestDelegate"/>.</param>
/// <param name="options">The <see cref="BoxOfficeProductOptions"/>.</param>
public class SignInCallbackMiddleware(RequestDelegate next, IOptions<BoxOfficeProductOptions> options)
{
    /// <summary>
    /// Handles a request.
    /// </summary>
    /// <param name="http">The <see cref="HttpContext"/> of the request.</param>
    /// <param name="calls">The <see cref="SignInCalls"/> that receive the callback.</param>
    /// <returns>A <see cref="Task"/> for continuation.</returns>
    public Task InvokeAsync(HttpContext http, SignInCalls calls) =>
        HttpMethods.IsPost(http.Request.Method) && http.Request.Path.Equals(options.Value.SignInPath, StringComparison.OrdinalIgnoreCase)
            ? Answer(http, calls)
            : next(http);

    static async Task Answer(HttpContext http, SignInCalls calls) => await (await calls.Receive(http)).ExecuteAsync(http);
}
