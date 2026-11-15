// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Cratis.Showrunner.StageManager.Understudies.Reporting;

/// <summary>
/// Serves the Prometheus metrics of the Understudy at <c>/metrics</c> on the port in <c>Metrics:Port</c> (<c>Metrics__Port</c> in the environment), 9090 by default.
/// A port of 0 turns the endpoint off.
/// </summary>
/// <param name="configuration">The configuration.</param>
/// <param name="exporter">The exporter.</param>
/// <param name="logger">The logger.</param>
public class PrometheusEndpoint(IConfiguration configuration, PrometheusExporter exporter, ILogger<PrometheusEndpoint> logger) : BackgroundService
{
    /// <summary>
    /// The port when none is configured.
    /// </summary>
    public const int DefaultPort = 9090;

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var port = configuration.GetValue("Metrics:Port", DefaultPort);
        if (port <= 0)
        {
            return;
        }

        using var listener = new HttpListener();
        listener.Prefixes.Add($"http://+:{port}/");
        try
        {
            listener.Start();
        }
        catch (HttpListenerException exception)
        {
            logger.MetricsEndpointNotStarted(port, exception.GetType().Name);
            return;
        }

        stoppingToken.Register(listener.Stop);
        while (!stoppingToken.IsCancellationRequested)
        {
            HttpListenerContext context;
            try
            {
                context = await listener.GetContextAsync();
            }
            catch (Exception exception) when (exception is HttpListenerException or ObjectDisposedException)
            {
                return;
            }

            var found = context.Request.Url?.AbsolutePath == "/metrics";
            var body = Encoding.UTF8.GetBytes(found ? exporter.Render() : string.Empty);
            context.Response.StatusCode = found ? 200 : 404;
            context.Response.ContentType = "text/plain; version=0.0.4; charset=utf-8";
            await context.Response.OutputStream.WriteAsync(body, stoppingToken);
            context.Response.Close();
        }
    }
}
