// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Understudies.Catalog;

/// <summary>
/// What a pruning pass did.
/// </summary>
/// <param name="Kept">The number of runs that were kept.</param>
/// <param name="Pruned">The number of runs that no tier keeps, or that are older than the maximum age, and were removed.</param>
/// <param name="Orphans">The number of runs without a manifest, left by a run that could not clean up after itself, that were removed.</param>
public record PruneResult(int Kept, int Pruned, int Orphans);
