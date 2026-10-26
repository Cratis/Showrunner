// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net;
using System.Text;
using System.Text.Json;
using Cratis.Arc;
using Cratis.Chronicle.Connections;
using Cratis.Chronicle.Registrations;
using Cratis.Showrunner.Usher.Access.OnBehalf;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cratis.Showrunner.Usher.Integration.given;

/// <summary>
/// An application hosting Usher behind AuthProxy: Arc with Chronicle, trusting AuthProxy's forwarded identity headers,
/// Usher's on-behalf endpoints mapped and paired with AuthProxy's signing key, served in memory by a test server.
/// </summary>
/// <param name="app">The running application.</param>
/// <param name="name">The name of its event store.</param>
public sealed class WebApplicationWithUsher(WebApplication app, EventStoreName name) : IAsyncDisposable
{
    /// <summary>
    /// The base address AuthProxy calls the application on in these specifications.
    /// </summary>
    public static readonly Uri BaseUrl = new("http://localhost");

    /// <summary>
    /// Gets the name of the application's event store.
    /// </summary>
    public EventStoreName Name => name;

    /// <summary>
    /// Gets the HTTP client that reaches the application.
    /// </summary>
    public HttpClient Http { get; } = app.GetTestClient();

    /// <summary>
    /// Starts the application.
    /// </summary>
    /// <param name="infrastructure">The Chronicle kernel.</param>
    /// <param name="deskStore">The desk store the application exchanges events with.</param>
    /// <param name="authProxy">The AuthProxy whose tokens the application accepts.</param>
    /// <returns>The running application.</returns>
    /// <exception cref="InvalidOperationException">The application's event store did not register with the kernel.</exception>
    public static async Task<WebApplicationWithUsher> Start(ChronicleInfrastructure infrastructure, EventStoreName deskStore, AuthProxy authProxy)
    {
        var name = new EventStoreName($"Ledger-{Guid.NewGuid():N}");
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.AddCratisArc(
            options =>
            {
                // AuthProxy strips and sets the forwarded identity headers, and Usher's routes are the ones its
                // generated proxies call: without the three namespace segments they skip (Cratis.Showrunner.Usher)
                // and without the command name.
                options.TrustForwardedIdentityHeaders = true;
                options.OnBehalfOf = new()
                {
                    // Arc verifies AuthProxy's assertion on every forwarded request; Usher uses the same issuer,
                    // audience and keys for AuthProxy's calls to its on-behalf endpoints.
                    Enabled = true,
                    Issuer = AuthProxy.Issuer,
                    Audience = AuthProxy.Audience,
                    Service = AuthProxy.Service,
                    SigningKeys = [new() { KeyId = AuthProxy.KeyId, PublicKeyPem = authProxy.PublicKeyPem }]
                };
                options.GeneratedApis.SegmentsToSkipForRoute = 3;
                options.GeneratedApis.IncludeCommandNameInRoute = false;
            },
            arc => arc.WithChronicle(options =>
            {
                options.ConnectionString = new(infrastructure.ChronicleConnectionString);
                options.EventStore = name;
            }));
        builder.Services.AddMicrosoftIdentityPlatformIdentityAuthentication();
        builder.Services.AddAuthorization();

        // Arc binds discovered interfaces to their implementations by convention, and its discovery takes in every
        // Cratis.* assembly because this application's own assemblies are Cratis.Showrunner.*. That binds Chronicle's
        // internal connection, which only Chronicle's client may create, so the binding is taken back out.
        builder.Services.RemoveAll<IChronicleConnection>();
        builder.AddUsher(usher =>
        {
            usher.ApplicationName = "Ledger";
            usher.DeskStores = [deskStore];
            usher.PollingInterval = TimeSpan.FromMilliseconds(500);
            usher.OnBehalf = new OnBehalfOptions
            {
                BaseUrl = BaseUrl,
                AgentIdentityProviders = ["usher-agents"]
            };
        });

        if (Environment.GetEnvironmentVariable("USHER_INTEGRATION_LOG") is { Length: > 0 } log)
        {
            builder.Logging.AddProvider(new FileLoggerProvider(log));
        }

        var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseCratisArc();
        app.MapUsher();
        await app.StartAsync();

        var application = new WebApplicationWithUsher(app, name);
        var registration = await (await application.In(EventStoreNamespaceName.Default)).WaitForRegistration(TimeSpan.FromSeconds(30));
        if (!registration.IsSuccess)
        {
            throw new InvalidOperationException($"Application store {name} did not register.", registration.Failure);
        }

        return application;
    }

    /// <summary>
    /// Gets the application's event store in a namespace.
    /// </summary>
    /// <param name="namespace">The namespace.</param>
    /// <returns>The event store.</returns>
    public Task<IEventStore> In(EventStoreNamespaceName @namespace) =>
        app.Services.GetRequiredService<IChronicleClient>().GetEventStore(name, @namespace);

    /// <summary>
    /// Executes a command as a signed-in user, the way AuthProxy forwards it.
    /// </summary>
    /// <param name="route">The command's route.</param>
    /// <param name="command">The command.</param>
    /// <param name="tenant">The tenant.</param>
    /// <param name="principal">The user, as a client principal.</param>
    /// <returns>The status and the command result, as JSON.</returns>
    public Task<(HttpStatusCode Status, JsonElement Result)> Execute(string route, object command, EventStoreNamespaceName tenant, ForwardedUser principal) =>
        Execute(route, command, tenant, principal, _ => { });

    /// <summary>
    /// Executes a command as a signed-in user, with the headers AuthProxy forwards and any more a caller adds.
    /// </summary>
    /// <param name="route">The command's route.</param>
    /// <param name="command">The command.</param>
    /// <param name="tenant">The tenant.</param>
    /// <param name="principal">The user, as a client principal.</param>
    /// <param name="headers">Adds headers beside the user's, given the request and the exact <c>x-ms-client-principal</c> value.</param>
    /// <returns>The status and the command result, as JSON.</returns>
    public async Task<(HttpStatusCode Status, JsonElement Result)> Execute(string route, object command, EventStoreNamespaceName tenant, ForwardedUser principal, Action<(HttpRequestMessage Request, string PrincipalHeader)> headers)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, route)
        {
            Content = new StringContent(JsonSerializer.Serialize(command, JsonSerializerOptions.Web), Encoding.UTF8, "application/json")
        };
        var principalHeader = AuthProxy.Encode(principal);
        request.Headers.Add("x-cratis-tenant-id", tenant.Value);
        request.Headers.Add("x-ms-client-principal", principalHeader);
        request.Headers.Add("x-ms-client-principal-id", principal.UserId);
        request.Headers.Add("x-ms-client-principal-name", principal.UserDetails);
        headers((request, principalHeader));

        using var response = await Http.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();
        return (response.StatusCode, JsonDocument.Parse(body.Length == 0 ? "{}" : body).RootElement.Clone());
    }

    /// <summary>
    /// Calls one of Usher's on-behalf endpoints as AuthProxy does.
    /// </summary>
    /// <param name="call">The <see cref="AuthProxyCall"/>.</param>
    /// <returns>The response.</returns>
    public async Task<HttpResponseMessage> Send(AuthProxyCall call)
    {
        using var request = new HttpRequestMessage(call.Method, call.Target);
        if (call.Body is { } body)
        {
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");
        }

        request.Headers.Authorization = new("Bearer", call.Token);
        request.Headers.Add("x-cratis-tenant-id", call.Tenant);
        request.Headers.Add("Tenant-ID", call.Tenant);
        request.Headers.Add("x-cratis-on-behalf-actor", call.ActorHeader);
        return await Http.SendAsync(request);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        Http.Dispose();
        await app.StopAsync();
        await app.DisposeAsync();
    }
}
