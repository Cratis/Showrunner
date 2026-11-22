// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Playbill.Endpoints;

/// <summary>
/// The list of releases.
/// </summary>
/// <param name="Current">The version the application is running; <see langword="null"/> when it is not known.</param>
/// <param name="Releases">The releases, the newest first.</param>
public record ReleaseListResponse(string? Current, IEnumerable<ReleaseSummaryResponse> Releases);
