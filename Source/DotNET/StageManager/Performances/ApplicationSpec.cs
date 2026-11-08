// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// The application a <see cref="Performance"/> runs.
/// </summary>
public class ApplicationSpec
{
    /// <summary>
    /// Gets or sets the image of the rendered application.
    /// </summary>
    [Required]
    [Description("The image of the rendered application, or the Stage runtime image (cratis/stage:VERSION) when published is set.")]
    public string Image { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the names of the application's event stores.
    /// </summary>
    [Description("The names of the application's event stores. The first one is the store Ante observes.")]
    public IList<string> EventStores { get; set; } = [];

    /// <summary>
    /// Gets or sets the port the application listens on.
    /// </summary>
    [Description("The port the application listens on.")]
    public int Port { get; set; } = 8080;

    /// <summary>
    /// Gets or sets the number of replicas.
    /// </summary>
    [Description("The number of replicas.")]
    public int Replicas { get; set; } = 1;

    /// <summary>
    /// Gets or sets the names of the sinks the application stores read models and graphs in. The Production must configure each one.
    /// </summary>
    [Description("The names of the sinks a traditional application stores read models and graphs in. The Production named by productionRef must configure each one.")]
    public IList<string> Sinks { get; set; } = [];

    /// <summary>
    /// Gets or sets the published Stage application settings. When set, the application runs as a published Stage application.
    /// </summary>
    [Description("Runs the application as a published Stage application: image is the Stage runtime image and the package comes from a ConfigMap.")]
    public PublishedSpec? Published { get; set; }
}
