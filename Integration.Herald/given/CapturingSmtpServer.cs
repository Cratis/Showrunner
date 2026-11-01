// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using MimeKit;
using SmtpServer;
using SmtpServer.Protocol;
using SmtpServer.Storage;

namespace Cratis.Showrunner.Herald.Integration.given;

/// <summary>
/// A mail server running inside the test process that keeps what it is sent.
/// </summary>
/// <param name="port">The port to listen on; zero picks a free one.</param>
public sealed class CapturingSmtpServer(int port = 0) : IAsyncDisposable
{
    readonly CancellationTokenSource _stop = new();
    readonly ConcurrentQueue<MimeMessage> _received = new();
    Task? _running;

    /// <summary>
    /// Gets the port the server listens on.
    /// </summary>
    public int Port { get; } = port == 0 ? FreePort() : port;

    /// <summary>
    /// Gets the messages the server was sent.
    /// </summary>
    public IReadOnlyCollection<MimeMessage> Received => _received;

    /// <summary>
    /// Starts listening.
    /// </summary>
    public void Start()
    {
        var options = new SmtpServerOptionsBuilder()
            .ServerName("localhost")
            .Endpoint(endpoint => endpoint.Endpoint(new IPEndPoint(IPAddress.Loopback, Port)))
            .Build();
        var services = new SmtpServer.ComponentModel.ServiceProvider();
        services.Add(new Store(_received));
        _running = new SmtpServer.SmtpServer(options, services).StartAsync(_stop.Token);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await _stop.CancelAsync();
        if (_running is not null)
        {
            try
            {
                await _running;
            }
            catch (OperationCanceledException)
            {
            }
        }

        _stop.Dispose();
    }

    static int FreePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }

    sealed class Store(ConcurrentQueue<MimeMessage> received) : MessageStore
    {
        public override async Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
        {
            await using var stream = new MemoryStream();
            foreach (var segment in buffer)
            {
                await stream.WriteAsync(segment, cancellationToken);
            }

            stream.Position = 0;
            received.Enqueue(await MimeMessage.LoadAsync(stream, cancellationToken));
            return SmtpResponse.Ok;
        }
    }
}
