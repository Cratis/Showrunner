// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net;
using System.Net.Sockets;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;

namespace Cratis.Showrunner.Herald.Integration.given;

/// <summary>
/// One real Chronicle kernel with its embedded MongoDB, shared by every specification in <see cref="ChronicleCollection"/>.
/// Isolation between specifications comes from unique event store names, not from a fresh container.
/// </summary>
/// <remarks>
/// The image defaults to the server release matching the pinned client; override it with <c>HERALD_CHRONICLE_IMAGE</c>.
/// </remarks>
public sealed class ChronicleInfrastructure : IAsyncLifetime
{
    /// <summary>
    /// The kernel image used unless <c>HERALD_CHRONICLE_IMAGE</c> names another.
    /// </summary>
    public const string DefaultImage = "cratis/chronicle:19.155.0-development";

    const ushort ChroniclePort = 35000;
    const ushort HttpPort = 8080;
    const ushort MongoDBPort = 27017;
    const int MaxStartAttempts = 3;

    IContainer? _container;
    int _chroniclePort;

    /// <summary>
    /// Gets the kernel started for <see cref="ChronicleCollection"/>. Specifications reach it statically: the shared
    /// specification runner only runs Establish and Because once for classes with a parameterless constructor.
    /// </summary>
    /// <exception cref="InvalidOperationException">The specification is not in the collection.</exception>
    public static ChronicleInfrastructure Current
    {
        get => field ?? throw new InvalidOperationException($"Specifications using the kernel must be in [Collection({nameof(ChronicleCollection)}.{nameof(ChronicleCollection.Name)})].");
        private set;
    }

    /// <summary>
    /// Gets the kernel image to run.
    /// </summary>
    public static string Image => Environment.GetEnvironmentVariable("HERALD_CHRONICLE_IMAGE") is { Length: > 0 } image ? image : DefaultImage;

    /// <summary>
    /// Gets the connection string of the kernel.
    /// </summary>
    public string ChronicleConnectionString => $"chronicle://localhost:{_chroniclePort}/?skipTlsValidation=true";

    /// <inheritdoc/>
    public async Task InitializeAsync()
    {
        // A free port is only known to be free when it is picked; another process can take it before Docker binds it.
        for (var attempt = 1; ; attempt++)
        {
            var container = CreateContainer();
            try
            {
                await container.StartAsync();
                _container = container;
                Current = this;
                return;
            }
            catch (Exception exception) when (attempt < MaxStartAttempts && IsPortBindFailure(exception))
            {
                await container.DisposeAsync();
            }
        }
    }

    /// <inheritdoc/>
    public async Task DisposeAsync()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }

    internal async Task<string> RegistrationDiagnostics(string description, TimeSpan elapsed, TimeSpan timeout)
    {
        var health = await HealthAtTimeout();
        var logs = await ContainerLogTail();
        var diagnostics = string.Join(
            Environment.NewLine,
            description,
            $"Waited {elapsed.TotalSeconds:F1} seconds for Chronicle registration (limit: {timeout.TotalSeconds:F1} seconds).",
            $"Kernel image: {Image}; container state: {_container?.State.ToString() ?? "unavailable"}.",
            health,
            logs);
        await Console.Error.WriteLineAsync(diagnostics);
        return diagnostics;
    }

    static string LogTail(string logs)
    {
        // Limit both line count and characters, including logs containing a single very long line.
        var bounded = logs.Length > 16000 ? logs[^16000..] : logs;
        return string.Join('\n', bounded.Split('\n').TakeLast(100));
    }

    async Task<string> HealthAtTimeout()
    {
        var uri = new Uri($"https://localhost:{_chroniclePort}/health");
        try
        {
#pragma warning disable MA0039, CA5400 // Test-only: accept the kernel's self-signed development certificate.
            using var handler = new HttpClientHandler { ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator };
            using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(2) };
#pragma warning restore MA0039, CA5400
            using var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
            return $"GET {uri}: {(int)response.StatusCode} {response.StatusCode}";
        }
        catch (Exception exception)
        {
            return $"GET {uri}: {exception.GetType().Name}: {LogTail(exception.Message)}";
        }
    }

    async Task<string> ContainerLogTail()
    {
        if (_container is null)
        {
            return "Chronicle container logs unavailable: container was not started.";
        }

        try
        {
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var (stdout, stderr) = await _container.GetLogsAsync(ct: cancellation.Token);
            return $"Chronicle stdout (last 100 lines, at most 16000 characters):\n{LogTail(stdout)}\nChronicle stderr (last 100 lines, at most 16000 characters):\n{LogTail(stderr)}";
        }
        catch (Exception exception)
        {
            return $"Chronicle container logs unavailable: {exception.GetType().Name}: {LogTail(exception.Message)}";
        }
    }

    static bool IsPortBindFailure(Exception exception)
    {
        var description = exception.ToString();
        return description.Contains("address already in use", StringComparison.OrdinalIgnoreCase) ||
            description.Contains("port is already allocated", StringComparison.OrdinalIgnoreCase);
    }

    static int FreePort(HashSet<int> reserved)
    {
        while (true)
        {
            using var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            if (reserved.Add(port))
            {
                return port;
            }
        }
    }

    IContainer CreateContainer()
    {
        var reserved = new HashSet<int>();
        _chroniclePort = FreePort(reserved);
        return new ContainerBuilder(Image)
            .WithPortBinding(_chroniclePort, ChroniclePort)
            .WithPortBinding(FreePort(reserved), HttpPort)
            .WithPortBinding(FreePort(reserved), MongoDBPort)
            .WithLabel("cratis.showrunner.integration", "true")
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilInternalTcpPortIsAvailable(MongoDBPort)
                .AddCustomWaitStrategy(new ChronicleHealthWait(ChroniclePort, HttpPort)))
            .Build();
    }

    /// <summary>
    /// Waits for the kernel's health endpoint: over TLS on the gRPC port on current servers, over HTTP on older ones.
    /// </summary>
    /// <param name="tlsPort">The gRPC port, which serves health over TLS.</param>
    /// <param name="httpPort">The HTTP port older servers serve health on.</param>
    sealed class ChronicleHealthWait(ushort tlsPort, ushort httpPort) : IWaitUntil
    {
        public async Task<bool> UntilAsync(IContainer container)
        {
#pragma warning disable MA0039, CA5400 // Test-only: accept the kernel's self-signed development certificate.
            using var handler = new HttpClientHandler { ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator };
            using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(2) };
#pragma warning restore MA0039, CA5400
            foreach (var uri in new[]
            {
                new Uri($"https://{container.Hostname}:{container.GetMappedPublicPort(tlsPort)}/health"),
                new Uri($"http://{container.Hostname}:{container.GetMappedPublicPort(httpPort)}/health"),
            })
            {
                try
                {
                    using var response = await client.GetAsync(uri);
                    if (response.IsSuccessStatusCode)
                    {
                        return true;
                    }
                }
                catch (HttpRequestException)
                {
                }
                catch (TaskCanceledException)
                {
                }
            }

            return false;
        }
    }
}
