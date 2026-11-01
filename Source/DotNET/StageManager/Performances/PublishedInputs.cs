// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// What a published Stage application needs on top of the common inputs: the storage, the Notes token and the release to run.
/// </summary>
/// <param name="Storage">The Secret holding the MongoDB connection string.</param>
/// <param name="NotesToken">The Secret holding the collector token, when Notes is enabled.</param>
/// <param name="Release">The release to deploy.</param>
public record PublishedInputs(ResolvedSecretRef Storage, ResolvedSecretRef? NotesToken, PublishedRelease Release);
