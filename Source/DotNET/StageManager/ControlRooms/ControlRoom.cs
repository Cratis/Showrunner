// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.ControlRooms;

/// <summary>
/// An outbound connection to a control plane. Schema only.
/// </summary>
[KubernetesEntity(Group = StageManagerApi.Group, ApiVersion = StageManagerApi.Version, Kind = "ControlRoom", PluralName = "controlrooms")]
public partial class ControlRoom : CustomKubernetesEntity<ControlRoomSpec, ControlRoomStatus>;
