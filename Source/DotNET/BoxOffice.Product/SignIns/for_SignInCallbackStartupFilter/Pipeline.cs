// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.Product.Activity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice.Product.SignIns.for_SignInCallbackStartupFilter;

/// <summary>
/// An application pipeline built through the startup filter whose application answers every request not found.
/// </summary>
public static class Pipeline
{
    public static async Task<(int Callback, int Other, int WrongMethod)> Run(BoxOfficeProductOptions options)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(Options.Create(options));
        services.AddSingleton(Substitute.For<IReportActivity>());
        services.AddScoped<SignInCalls>();
        await using var provider = services.BuildServiceProvider();

        var filter = new SignInCallbackStartupFilter(Options.Create(options));
        var application = filter.Configure(app => app.Run(http =>
        {
            http.Response.StatusCode = StatusCodes.Status404NotFound;
            return Task.CompletedTask;
        }));
        var builder = new ApplicationBuilder(provider);
        application(builder);
        var pipeline = builder.Build();

        return (await Send(pipeline, provider, HttpMethods.Post, options.SignInPath), await Send(pipeline, provider, HttpMethods.Post, "/somewhere/else"), await Send(pipeline, provider, HttpMethods.Get, options.SignInPath));
    }

    static async Task<int> Send(RequestDelegate pipeline, IServiceProvider provider, string method, string path)
    {
        await using var scope = provider.CreateAsyncScope();
        var http = new DefaultHttpContext { RequestServices = scope.ServiceProvider };
        http.Request.Method = method;
        http.Request.Path = path;
        await pipeline(http);
        return http.Response.StatusCode;
    }
}
#endif
