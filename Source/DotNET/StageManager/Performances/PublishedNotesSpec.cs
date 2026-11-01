// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// How a published Stage application sends its notes to the collecting side.
/// </summary>
public class PublishedNotesSpec
{
    /// <summary>
    /// Gets or sets whether the Notes backend is hosted.
    /// </summary>
    [Description("Whether the application hosts the Notes backend.")]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the URL of the collector.
    /// </summary>
    [Description("The absolute http or https URL of the collecting host.")]
    public string CollectorUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Secret holding the bearer secret of the publication.
    /// </summary>
    [Description("The Secret holding the bearer secret of the publication (key token unless set). It is mounted as a file.")]
    public SecretKeyReference CollectorTokenSecretRef { get; set; } = new();

    /// <summary>
    /// Gets or sets the publication the application runs as.
    /// </summary>
    [Description("The publication the application runs as.")]
    public string PublicationId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tenant the collector knows the publication by.
    /// </summary>
    [Description("The tenant the collector knows the publication by. Omit when the application's namespace is named like the tenant.")]
    public string? CollectorTenantId { get; set; }

    /// <summary>
    /// Gets or sets the name the receiving side sees on every note.
    /// </summary>
    [Description("The application name the receiving side sees on every note.")]
    public string ApplicationName { get; set; } = string.Empty;
}
