// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageOperator.Configuration;

/// <summary>
/// Represents how the Stage operator is configured. Bound from the <c>StageOperator</c> section of
/// <c>appsettings.json</c> or from <c>StageOperator__*</c> environment variables.
/// </summary>
public class StageOperatorOptions
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = "StageOperator";

    /// <summary>Gets or sets the domain applications are published under; the host is <c>&lt;publishedName&gt;.&lt;BaseDomain&gt;</c>.</summary>
    public string BaseDomain { get; set; } = "cratis.app";

    /// <summary>Gets or sets the ingress class name. Empty leaves the cluster default.</summary>
    public string IngressClassName { get; set; } = string.Empty;

    /// <summary>Gets or sets the name of the cert-manager cluster issuer; sets the <c>cert-manager.io/cluster-issuer</c> annotation on the ingress.</summary>
    public string TlsIssuerAnnotation { get; set; } = string.Empty;

    /// <summary>Gets or sets the name of a wildcard TLS secret. When set it is used instead of a certificate per host.</summary>
    public string TlsSecretName { get; set; } = string.Empty;

    /// <summary>Gets or sets how long the data of an expired or deleted application is kept.</summary>
    public TimeSpan ExpiredDataRetention { get; set; } = TimeSpan.FromDays(7);

    /// <summary>Gets or sets the namespaces to watch. Empty watches every namespace.</summary>
    public IList<string> WatchNamespaces { get; set; } = [];

    /// <summary>Gets or sets how long a new image gets to become ready before the rollout is rolled back.</summary>
    public TimeSpan RolloutTimeout { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>Gets or sets how often resource usage is sampled.</summary>
    public TimeSpan MetricsSampleInterval { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>Gets or sets how often every resource is reconciled even when nothing changed.</summary>
    public TimeSpan ResyncInterval { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>Gets or sets the MongoDB image of the per-application database.</summary>
    public string MongoImage { get; set; } = "mongo:8";

    /// <summary>Gets or sets the storage class of the MongoDB volumes. Empty uses the cluster default.</summary>
    public string StorageClassName { get; set; } = string.Empty;

    /// <summary>Gets or sets the default size of the MongoDB volume.</summary>
    public string DefaultStorageSize { get; set; } = "5Gi";

    /// <summary>
    /// Gets or sets the port of the Notes collector that application pods may be reached on when Notes is enabled.
    /// Zero means none.
    /// </summary>
    public int NotesCollectorPort { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the ingress also carries an <c>ingress-nginx</c> <c>server-snippet</c>
    /// that answers the denied paths itself. Off by default: many clusters disable snippet annotations, and the
    /// denied paths are already routed to a backend without endpoints on every ingress controller.
    /// </summary>
    public bool DenyPathsWithSnippet { get; set; }
}
