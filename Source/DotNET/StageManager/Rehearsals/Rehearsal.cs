// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Rehearsals;

/// <summary>
/// Proves that backups restore by restoring them somewhere safe. Schema only.
/// </summary>
[KubernetesEntity(Group = StageManagerApi.Group, ApiVersion = StageManagerApi.Version, Kind = "Rehearsal", PluralName = "rehearsals")]
public partial class Rehearsal : CustomKubernetesEntity<RehearsalSpec, RehearsalStatus>;
