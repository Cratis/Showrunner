// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// The MongoDB metrics of a managed database: one <c>mongodb_exporter</c> per member, so Prometheus can see cache fill, connections and replication lag from inside.
/// </summary>
public class ProductionDatabaseMetrics
{
    /// <summary>
    /// The exporter image used when none is given.
    /// </summary>
    public const string DefaultImage = "percona/mongodb_exporter:0.44.0";

    /// <summary>
    /// The port the exporter serves metrics on.
    /// </summary>
    public const int Port = 9216;

    /// <summary>
    /// Gets or sets a value indicating whether an exporter runs for every member. Off by default.
    /// </summary>
    [Description("Runs one mongodb_exporter per member of a Managed database, as separate Deployments so a mongod is never rolled, with the diagnostic data and replica set status collectors only. Prometheus scrape annotations are set on the pods. Off by default. Turning it off later leaves the Deployments in place; delete them.")]
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the exporter image.
    /// </summary>
    [Description("The mongodb_exporter image. Defaults to a pinned version.")]
    public string Image { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the node labels the exporters must match.
    /// </summary>
    [Description("The node labels the exporters must match, so a fault in one cannot pressure a data node. Empty lets them run anywhere the Production's database members are not required to.")]
    public IDictionary<string, string> NodeSelector { get; set; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>
    /// Gets the image of the exporter, or the pinned default.
    /// </summary>
    /// <returns>The image.</returns>
    public string ImageOrDefault() => string.IsNullOrWhiteSpace(Image) ? DefaultImage : Image;
}
