// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net.Http.Headers;
#if !NET11_0_OR_GREATER
using System.Net.Http.Json;
#endif
using Cratis.Showrunner.Notes.Contracts.Transport;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Notes.Transport;

/// <summary>
/// Makes the application's calls to the collector over HTTP, authenticating each with the bearer secret.
/// </summary>
/// <param name="http">The HTTP client.</param>
/// <param name="token">Reads the secret.</param>
/// <param name="options">Notes' options.</param>
public class HttpCollectorClient(HttpClient http, CollectorToken token, IOptions<NotesOptions> options) : ICollectorClient
{
    /// <inheritdoc/>
    public async Task<ulong?> GetPosition(string source, CancellationToken cancellationToken = default)
    {
        using var response = await Call(HttpMethod.Get, $"{NotesTransportPaths.Position}{Query(source)}", null, cancellationToken);
        var position = await response.Content.ReadFromJsonAsync<NotesPosition>(NotesTransportJson.Options, cancellationToken);
        return position?.SequenceNumber;
    }

    /// <inheritdoc/>
    public async Task Send(string source, IReadOnlyList<PublishedNoteEvent> events, CancellationToken cancellationToken = default)
    {
        using var response = await Call(HttpMethod.Post, $"{NotesTransportPaths.Notes}{Query(source)}", JsonContent.Create(events, options: NotesTransportJson.Options), cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<NoteResponse>> GetResponses(ulong after, CancellationToken cancellationToken = default)
    {
        using var response = await Call(HttpMethod.Get, $"{NotesTransportPaths.Responses}?{NotesTransportPaths.AfterParameter}={after}", null, cancellationToken);
        return await response.Content.ReadFromJsonAsync<NoteResponse[]>(NotesTransportJson.Options, cancellationToken) ?? [];
    }

    static string Query(string source) =>
        source.Length == 0 ? string.Empty : $"?{NotesTransportPaths.SourceParameter}={Uri.EscapeDataString(source)}";

    async Task<HttpResponseMessage> Call(HttpMethod method, string relative, HttpContent? content, CancellationToken cancellationToken)
    {
        var configured = options.Value;
        var baseUrl = configured.CollectorUrl.TrimEnd('/');
        var publication = Uri.EscapeDataString(configured.PublicationId.Value);
        using var request = new HttpRequestMessage(method, $"{baseUrl}/.cratis/notes/v1/publications/{publication}/{relative}") { Content = content };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await token.Read(cancellationToken));
        var response = await http.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var status = response.StatusCode;
            response.Dispose();
            throw new CollectorRequestFailed(status);
        }

        return response;
    }
}
