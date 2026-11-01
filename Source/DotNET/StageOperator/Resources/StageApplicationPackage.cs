// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace Cratis.Showrunner.StageOperator.Resources;

/// <summary>Represents where the application package is delivered from.</summary>
public record StageApplicationPackage
{
    /// <summary>Gets the name of the ConfigMap, in the same namespace, whose key <c>package.tar.gz</c> holds the package archive.</summary>
    [JsonPropertyName("configMapRef")]
    public string? ConfigMapRef { get; init; }
}
