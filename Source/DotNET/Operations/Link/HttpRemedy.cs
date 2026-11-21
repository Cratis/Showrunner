// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net.Http.Headers;
using Cratis.Showrunner.Operations.Link.Protocol;

namespace Cratis.Showrunner.Operations.Link;

/// <summary>
/// Executes the request a Direct remedy pins - method, URL, fixed JSON body and bearer credential - exactly as the signed
/// configuration states it. The link only uses it when HTTP remedies are enabled explicitly.
/// </summary>
/// <param name="http">The HTTP client; it must not follow redirects.</param>
/// <param name="name">The name of the remedy it executes.</param>
public sealed class HttpRemedy(HttpClient http, string name) : IRemedy
{
    /// <inheritdoc/>
    public string Name => name;

    /// <inheritdoc/>
    public async Task<RemedyResult> Execute(RemedyInvocation invocation, CancellationToken cancellationToken)
    {
        var remedy = invocation.Remedy;
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cancellation.CancelAfter(remedy.Timeout > TimeSpan.Zero ? remedy.Timeout : TimeSpan.FromSeconds(10));
        try
        {
            var body = remedy.Parameters.ToDictionary(parameter => parameter.Name, parameter => parameter.Value, StringComparer.Ordinal);
            using var request = new HttpRequestMessage(remedy.Method == WatchdogHttpMethod.Put ? HttpMethod.Put : HttpMethod.Post, remedy.BaseUrl.TrimEnd('/') + remedy.Path)
            {
                Content = System.Net.Http.Json.JsonContent.Create(body, options: WatchdogJson.Options)
            };
            if (!string.IsNullOrEmpty(remedy.Credential) && invocation.Secrets.TryGetValue(remedy.Credential, out var secret))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", secret);
            }

            using var response = await http.SendAsync(request, cancellation.Token);
            var status = (int)response.StatusCode;
            return new RemedyResult(response.IsSuccessStatusCode, status, $"{request.Method} {remedy.Path} answered {status}");
        }
        catch (Exception error) when (error is HttpRequestException or OperationCanceledException && !cancellationToken.IsCancellationRequested)
        {
            return new RemedyResult(false, 0, $"{remedy.Path} could not be reached: {error.GetType().Name}");
        }
    }
}
