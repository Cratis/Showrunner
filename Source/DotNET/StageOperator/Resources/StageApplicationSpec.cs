// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace Cratis.Showrunner.StageOperator.Resources;

/// <summary>Represents the desired state of a published Stage application.</summary>
public record StageApplicationSpec
{
    /// <summary>Gets the publication this application belongs to in the publisher's system.</summary>
    [JsonPropertyName("publicationId")]
    public string PublicationId { get; init; } = string.Empty;

    /// <summary>Gets the organization namespace (tenant) the application belongs to.</summary>
    [JsonPropertyName("organization")]
    public string Organization { get; init; } = string.Empty;

    /// <summary>Gets the DNS label the application is published under; the host is <c>&lt;publishedName&gt;.&lt;baseDomain&gt;</c>.</summary>
    [JsonPropertyName("publishedName")]
    public string PublishedName { get; init; } = string.Empty;

    /// <summary>Gets the application version.</summary>
    [JsonPropertyName("version")]
    public string Version { get; init; } = string.Empty;

    /// <summary>Gets the exported Stage application image, with the package baked in.</summary>
    [JsonPropertyName("image")]
    public string Image { get; init; } = string.Empty;

    /// <summary>Gets how long the application runs.</summary>
    [JsonPropertyName("lifetime")]
    public StageApplicationLifetime? Lifetime { get; init; }

    /// <summary>Gets the sign-in proxy configuration. Required.</summary>
    [JsonPropertyName("authentication")]
    public StageApplicationAuthentication? Authentication { get; init; }

    /// <summary>Gets the Notes configuration.</summary>
    [JsonPropertyName("notes")]
    public StageApplicationNotes? Notes { get; init; }

    /// <summary>Gets the resources of the Stage container.</summary>
    [JsonPropertyName("resources")]
    public StageApplicationResources? Resources { get; init; }

    /// <summary>Gets the per-application MongoDB storage.</summary>
    [JsonPropertyName("storage")]
    public StageApplicationStorage? Storage { get; init; }
}
