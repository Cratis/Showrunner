// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// What has to be in hand before a Performance may roll out.
/// </summary>
/// <param name="Connection">The Chronicle connection Secret.</param>
/// <param name="Environment">The resolved Cratis environment, complete for this Performance.</param>
public record PreparedPerformance(ChronicleConnectionRef Connection, ResolvedEnvironment Environment);
