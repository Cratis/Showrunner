// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Understudies;

/// <summary>
/// How a Production is backed up. Schema only.
/// </summary>
[KubernetesEntity(Group = StageManagerApi.Group, ApiVersion = StageManagerApi.Version, Kind = "Understudy", PluralName = "understudies")]
public partial class Understudy : CustomKubernetesEntity<UnderstudySpec, UnderstudyStatus>;
