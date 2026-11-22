// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Playbill.Endpoints;

/// <summary>
/// The version the application is running.
/// </summary>
/// <param name="Version">The version; <see langword="null"/> when it is not known.</param>
/// <param name="Release">The note of that release; <see langword="null"/> when it has none.</param>
/// <param name="Newest">The newest release that has a note; <see langword="null"/> when none has.</param>
public record CurrentReleaseResponse(string? Version, ReleaseSummaryResponse? Release, string? Newest);
