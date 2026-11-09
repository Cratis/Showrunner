// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Nodes;

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// A cert-manager Certificate. Stage Manager creates it and does not own its definition, so it is left out of CRD generation.
/// </summary>
[Ignore]
[KubernetesEntity(Group = "cert-manager.io", ApiVersion = "v1", Kind = "Certificate", PluralName = "certificates")]
public class CertManagerCertificate : IKubernetesObject<V1ObjectMeta>
{
    /// <summary>
    /// The name of its custom resource definition, which tells whether the operator behind it is installed.
    /// </summary>
    public const string DefinitionName = "certificates.cert-manager.io";

    /// <inheritdoc/>
    public string ApiVersion { get; set; } = "cert-manager.io/v1";

    /// <inheritdoc/>
    public string Kind { get; set; } = "Certificate";

    /// <inheritdoc/>
    public V1ObjectMeta Metadata { get; set; } = new();

    /// <summary>
    /// Gets or sets the spec, as the owner of the definition documents it.
    /// </summary>
    [JsonPropertyName("spec")]
    public JsonObject Spec { get; set; } = [];

    /// <summary>
    /// Gets or sets the status the owner of the definition reports.
    /// </summary>
    [JsonPropertyName("status")]
    public JsonObject? Status { get; set; }
}
