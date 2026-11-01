// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Restores;

/// <summary>
/// Brings a backup back into an event store or namespace. Schema only.
/// </summary>
[KubernetesEntity(Group = StageManagerApi.Group, ApiVersion = StageManagerApi.Version, Kind = "Restore", PluralName = "restores")]
public partial class Restore : CustomKubernetesEntity<RestoreSpec, RestoreStatus>;
