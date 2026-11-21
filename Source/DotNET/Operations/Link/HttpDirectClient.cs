// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Cratis.Showrunner.Operations.Link.Protocol;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Link;

/// <summary>
/// The HTTP client of Direct's watchdog protocol. The credential is sent as a bearer token and never logged; the client it is
/// given must not follow redirects, so the token never follows one to another host.
/// </summary>
/// <param name="http">The HTTP client.</param>
/// <param name="baseUrl">The base URL of Direct.</param>
/// <param name="instanceName">The instance name.</param>
public sealed class HttpDirectClient(HttpClient http, Uri baseUrl, string instanceName) : IDirectClient
{
    /// <inheritdoc/>
    public Task<DirectAnswer<WatchdogClaimResponse>> Claim(string bearer, WatchdogClaimRequest request, CancellationToken cancellationToken) =>
        Send<WatchdogClaimResponse>(HttpMethod.Post, WatchdogProtocol.Claim, string.Empty, bearer, null, request, cancellationToken);

    /// <inheritdoc/>
    public Task<DirectAnswer<SignedWatchdogConfiguration>> Configuration(DirectCredentials credentials, long after, bool renew, CancellationToken cancellationToken) =>
        Send<SignedWatchdogConfiguration>(
            HttpMethod.Get,
            WatchdogProtocol.Configuration,
            $"?{WatchdogProtocol.AfterParameter}={after.ToString(CultureInfo.InvariantCulture)}&{WatchdogProtocol.RenewParameter}={(renew ? "true" : "false")}",
            credentials.Credential,
            credentials.Epoch,
            null,
            cancellationToken);

    /// <inheritdoc/>
    public Task<DirectAnswer<WatchdogConfigurationAcknowledgement>> Applied(DirectCredentials credentials, WatchdogConfigurationAcknowledgement acknowledgement, CancellationToken cancellationToken) =>
        Send<WatchdogConfigurationAcknowledgement>(HttpMethod.Post, WatchdogProtocol.ConfigurationApplied, string.Empty, credentials.Credential, credentials.Epoch, acknowledgement, cancellationToken);

    /// <inheritdoc/>
    public Task<DirectAnswer<WatchdogEventBatchReceipt>> Events(DirectCredentials credentials, WatchdogEventBatch batch, CancellationToken cancellationToken) =>
        Send<WatchdogEventBatchReceipt>(HttpMethod.Post, WatchdogProtocol.Events, string.Empty, credentials.Credential, credentials.Epoch, batch, cancellationToken);

    /// <inheritdoc/>
    public Task<DirectAnswer<WatchdogRemedyRequestReceipt>> RemedyRequest(DirectCredentials credentials, WatchdogRemedyRequest request, CancellationToken cancellationToken) =>
        Send<WatchdogRemedyRequestReceipt>(HttpMethod.Post, WatchdogProtocol.RemedyRequests, string.Empty, credentials.Credential, credentials.Epoch, request, cancellationToken);

    /// <inheritdoc/>
    public Task<DirectAnswer<WatchdogRemedyAuthorizations>> RemedyAuthorizations(DirectCredentials credentials, CancellationToken cancellationToken) =>
        Send<WatchdogRemedyAuthorizations>(HttpMethod.Get, WatchdogProtocol.RemedyAuthorizations, string.Empty, credentials.Credential, credentials.Epoch, null, cancellationToken);

    static DirectAnswer<T> Answer<T>(DirectOutcome outcome, string reason) => new(outcome, default, reason);

    static async Task<DirectAnswer<T>> Read<T>(HttpResponseMessage response, CancellationToken cancellationToken) => response.StatusCode switch
    {
        HttpStatusCode.OK => new DirectAnswer<T>(DirectOutcome.Ok, JsonSerializer.Deserialize<T>(await response.Content.ReadAsStringAsync(cancellationToken), WatchdogJson.Options), string.Empty),
        HttpStatusCode.NoContent => Answer<T>(DirectOutcome.NoContent, string.Empty),
        HttpStatusCode.BadRequest => Answer<T>(DirectOutcome.Rejected, Redaction.Summarize(await response.Content.ReadAsStringAsync(cancellationToken))),
        HttpStatusCode.Unauthorized => Answer<T>(DirectOutcome.Unauthorized, "the credential is not accepted"),
        HttpStatusCode.Conflict => Answer<T>(DirectOutcome.Fenced, Redaction.Summarize(await response.Content.ReadAsStringAsync(cancellationToken))),
        _ => Answer<T>(DirectOutcome.Unavailable, $"HTTP {(int)response.StatusCode}")
    };

    async Task<DirectAnswer<T>> Send<T>(HttpMethod method, string route, string query, string bearer, long? epoch, object? body, CancellationToken cancellationToken)
    {
        var address = new Uri(baseUrl.ToString().TrimEnd('/') + WatchdogProtocol.For(route, instanceName) + query);
        using var request = new HttpRequestMessage(method, address);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
        if (epoch is { } held)
        {
            request.Headers.Add(WatchdogProtocol.EpochHeader, held.ToString(CultureInfo.InvariantCulture));
        }

        if (body is not null)
        {
            request.Content = System.Net.Http.Json.JsonContent.Create(body, body.GetType(), options: WatchdogJson.Options);
        }

        try
        {
            using var response = await http.SendAsync(request, cancellationToken);
            return await Read<T>(response, cancellationToken);
        }
        catch (Exception error) when (error is HttpRequestException or TaskCanceledException or JsonException && !cancellationToken.IsCancellationRequested)
        {
            // The message of a transport error can carry the URL but never the credential; the type is enough for a log.
            return Answer<T>(DirectOutcome.Unavailable, error.GetType().Name);
        }
    }
}
