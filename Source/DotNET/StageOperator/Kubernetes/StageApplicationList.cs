// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;
using Cratis.Showrunner.StageOperator.Resources;

namespace Cratis.Showrunner.StageOperator.Kubernetes;

/// <summary>A list of <c>StageApplication</c> resources.</summary>
internal sealed class StageApplicationList
{
    /// <summary>Gets or sets the applications.</summary>
    [JsonPropertyName("items")]
    public List<StageApplication> Items { get; set; } = [];
}
