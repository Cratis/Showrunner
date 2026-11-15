// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Understudies.Catalog;

/// <summary>
/// Whether a backup is kept and by which tiers.
/// </summary>
/// <param name="Keep">Whether the backup is kept.</param>
/// <param name="Tiers">The tiers that keep it, such as <see cref="RetentionPolicy.HourlyTier"/>.</param>
public record RetentionVerdict(bool Keep, IReadOnlyList<string> Tiers);
