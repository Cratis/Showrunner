// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Showrunner.Operations.Cluster;

/// <summary>
/// One page of a GitHub list answer.
/// </summary>
/// <param name="Body">The JSON body.</param>
/// <param name="Next">The address of the next page from the <c>Link</c> header, or <see langword="null"/> when there is none.</param>
public sealed record GitHubPage(JsonElement Body, string? Next);
