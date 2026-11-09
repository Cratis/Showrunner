// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// What has to be in hand before a Performance may roll out.
/// </summary>
/// <param name="Connection">The Chronicle connection Secret, or null for a published Stage application, which runs its own kernel.</param>
/// <param name="Environment">The resolved Cratis environment, complete for this Performance.</param>
/// <param name="Storage">The Secret holding the storage connection string of a published Stage application.</param>
/// <param name="NotesToken">The Secret holding the Notes collector token of a published Stage application.</param>
/// <param name="ReadSide">The read side of the named sinks and the graph the Production gives the application, or null without a Production.</param>
public record PreparedPerformance(ChronicleConnectionRef? Connection, ResolvedEnvironment Environment, ResolvedSecretRef? Storage = null, ResolvedSecretRef? NotesToken = null, ApplicationReadSide? ReadSide = null);
