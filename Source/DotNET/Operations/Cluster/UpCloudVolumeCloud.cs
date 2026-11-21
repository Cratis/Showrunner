// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Cratis.Showrunner.Operations.Cluster;

/// <summary>
/// The UpCloud API as an <see cref="IVolumeCloud"/>. The credentials are only ever sent as the Authorization header and are
/// never part of an error; an error carries the method, the path and the HTTP status only.
/// </summary>
/// <param name="http">The HTTP client; its timeout bounds each call.</param>
/// <param name="username">The API user.</param>
/// <param name="password">The API password.</param>
/// <param name="clusterLabel">The label the CSI driver puts on the volumes it provisions.</param>
public sealed class UpCloudVolumeCloud(HttpClient http, string username, string password, string clusterLabel) : IVolumeCloud
{
    static readonly Uri _api = new("https://api.upcloud.com/1.3/");

    /// <summary>
    /// Reads the error code out of an UpCloud error answer.
    /// </summary>
    /// <param name="text">The body of the answer.</param>
    /// <returns>The error code, or <see langword="null"/> when the body is not an UpCloud error.</returns>
    public static string? ErrorCode(string text)
    {
        try
        {
            return JsonNode.Parse(text)?["error"]?["error_code"]?.GetValue<string>();
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<CloudVolume>> ListVolumes(CancellationToken cancellationToken)
    {
        var answer = await Send(HttpMethod.Get, "storage/normal", null, cancellationToken);
        return [.. (answer?["storages"]?["storage"] as JsonArray ?? []).Select(Volume)];
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<CloudAttachment>> Attachments(string volumeId, CancellationToken cancellationToken)
    {
        var detail = await Send(HttpMethod.Get, $"storage/{Uri.EscapeDataString(volumeId)}", null, cancellationToken);
        var attachments = new List<CloudAttachment>();
        foreach (var serverId in (detail?["storage"]?["servers"]?["server"] as JsonArray ?? []).Select(node => node?.GetValue<string>()).OfType<string>())
        {
            var server = (await Send(HttpMethod.Get, $"server/{Uri.EscapeDataString(serverId)}", null, cancellationToken))?["server"];
            var address = (server?["storage_devices"]?["storage_device"] as JsonArray ?? [])
                .Where(device => device?["storage"]?.GetValue<string>() == volumeId)
                .Select(device => device?["address"]?.GetValue<string>())
                .FirstOrDefault() ?? throw new InvalidOperationException("an attached volume has no device address");
            attachments.Add(new CloudAttachment(serverId, server?["state"]?.GetValue<string>() == "started", address));
        }

        return attachments;
    }

    /// <inheritdoc/>
    public async Task Detach(CloudAttachment attachment, CancellationToken cancellationToken)
    {
        var body = new JsonObject { ["storage_device"] = new JsonObject { ["address"] = attachment.Address } };
        await Send(HttpMethod.Post, $"server/{Uri.EscapeDataString(attachment.ServerId)}/storage/detach", body, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> Delete(string volumeId, CancellationToken cancellationToken)
    {
        try
        {
            await Send(HttpMethod.Delete, $"storage/{Uri.EscapeDataString(volumeId)}", null, cancellationToken);
            return true;
        }
        catch (UpCloudException error) when (error.Code == "STORAGE_ATTACHED")
        {
            return false;
        }
    }

    CloudVolume Volume(JsonNode? storage)
    {
        var labels = storage?["labels"];
        var list = (labels as JsonObject)?["label"] as JsonArray ?? labels as JsonArray ?? [];
        var cluster = list.Where(label => label?["key"]?.GetValue<string>() == clusterLabel).Select(label => label?["value"]?.GetValue<string>()).FirstOrDefault();
        return new CloudVolume(
            storage?["uuid"]?.GetValue<string>() ?? throw new InvalidOperationException("a volume has no id"),
            storage["title"]?.GetValue<string>() ?? string.Empty,
            DateTimeOffset.Parse(storage["created"]?.GetValue<string>() ?? throw new InvalidOperationException("a volume has no creation time"), CultureInfo.InvariantCulture),
            (int)(storage["size"]?.GetValue<double>() ?? 0),
            cluster);
    }

    async Task<JsonNode?> Send(HttpMethod method, string path, JsonNode? body, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, new Uri(_api, path));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}")));
        if (body is not null)
        {
            request.Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json");
        }

        using var response = await http.SendAsync(request, cancellationToken);
        var text = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new UpCloudException($"{method} {path}: HTTP {(int)response.StatusCode}", ErrorCode(text));
        }

        return string.IsNullOrWhiteSpace(text) ? null : JsonNode.Parse(text);
    }
}
