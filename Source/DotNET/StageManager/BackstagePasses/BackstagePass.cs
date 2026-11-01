// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.BackstagePasses;

/// <summary>
/// Who may use the Stage Manager API. Schema only.
/// </summary>
[KubernetesEntity(Group = StageManagerApi.Group, ApiVersion = StageManagerApi.Version, Kind = "BackstagePass", PluralName = "backstagepasses")]
public partial class BackstagePass : CustomKubernetesEntity<BackstagePassSpec, BackstagePassStatus>;
