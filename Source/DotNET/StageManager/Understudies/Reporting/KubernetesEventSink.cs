// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Cryptography;
using System.Text;
using Cratis.Json;

namespace Cratis.Showrunner.StageManager.Understudies.Reporting;

/// <summary>
/// Publishes facts as Kubernetes events on the Understudy, for in-cluster consumers: the reason is the name of the event type and the message is the event as JSON.
/// The name of the Kubernetes event comes from the id of the fact, so a fact that is published twice is created once.
/// </summary>
/// <param name="cluster">The cluster.</param>
/// <param name="time">The time provider.</param>
public class KubernetesEventSink(IUnderstudyCluster cluster, TimeProvider time) : IUnderstudyEventSink
{
    static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web) { Converters = { new ConceptAsJsonConverterFactory() } };

    /// <summary>
    /// Gets the name of the Kubernetes event of a fact.
    /// </summary>
    /// <param name="understudy">The name of the Understudy.</param>
    /// <param name="factId">The id of the fact.</param>
    /// <returns>The name.</returns>
    public static string NameOf(string understudy, string factId) =>
        $"{understudy}.{Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(factId)))[..20]}";

    /// <inheritdoc/>
    public async Task PublishAsync(Understudy understudy, UnderstudyFact fact, CancellationToken cancellationToken)
    {
        var now = time.GetUtcNow().UtcDateTime;
        await cluster.TryCreateAsync(
            new Corev1Event
            {
                Metadata = new()
                {
                    Name = NameOf(understudy.Metadata.Name, fact.Id),
                    NamespaceProperty = understudy.Metadata.NamespaceProperty
                },
                InvolvedObject = new()
                {
                    ApiVersion = $"{StageManagerApi.Group}/{StageManagerApi.Version}",
                    Kind = "Understudy",
                    Name = understudy.Metadata.Name,
                    NamespaceProperty = understudy.Metadata.NamespaceProperty,
                    Uid = understudy.Metadata.Uid
                },
                Reason = fact.Type,
                Message = JsonSerializer.Serialize(fact.Payload, fact.Payload.GetType(), _options),
                Type = fact.Warning ? "Warning" : "Normal",
                ReportingComponent = "stage-manager",
                ReportingInstance = "stage-manager",
                Action = fact.Type,
                FirstTimestamp = now,
                LastTimestamp = now,
                Count = 1
            },
            cancellationToken);
    }
}
