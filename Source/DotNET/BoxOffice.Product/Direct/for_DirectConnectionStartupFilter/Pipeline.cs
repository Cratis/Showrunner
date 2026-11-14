// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnectionStartupFilter;

/// <summary>
/// An application pipeline built through the startup filter whose application answers every request not found.
/// </summary>
public static class Pipeline
{
    public static async Task<(int Get, int Post, int Other)> Run(BoxOfficeProductOptions options, IDirectEnrollment? enrollment)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(Options.Create(options));
        services.AddSingleton(Substitute.For<IDirectLinkRecord>());
        if (enrollment is not null)
        {
            services.AddSingleton(enrollment);
        }

        services.AddSingleton<AttemptLimiter>();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<DirectForgery>();
        services.AddSingleton<DirectConnection>();
        services.AddScoped<DirectConnectionCalls>();
        await using var provider = services.BuildServiceProvider();

        var filter = new DirectConnectionStartupFilter(Options.Create(options));
        var application = filter.Configure(app => app.Run(http =>
        {
            http.Response.StatusCode = StatusCodes.Status404NotFound;
            return Task.CompletedTask;
        }));
        var builder = new ApplicationBuilder(provider);
        application(builder);
        var pipeline = builder.Build();

        return (await Send(pipeline, provider, HttpMethods.Get, options.Direct.Path), await Send(pipeline, provider, HttpMethods.Post, options.Direct.Path), await Send(pipeline, provider, HttpMethods.Get, "/somewhere/else"));
    }

    static async Task<int> Send(RequestDelegate pipeline, IServiceProvider provider, string method, string path)
    {
        await using var scope = provider.CreateAsyncScope();
        var http = new DefaultHttpContext { RequestServices = scope.ServiceProvider };
        http.Request.Method = method;
        http.Request.Path = path;
        http.Response.Body = new MemoryStream();
        await pipeline(http);
        return http.Response.StatusCode;
    }
}
#endif
