// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// One application running in one Cratis environment: the application, its AuthProxy and, when it has onboarding, its own Ante.
/// </summary>
[KubernetesEntity(Group = StageManagerApi.Group, ApiVersion = StageManagerApi.Version, Kind = "Performance", PluralName = "performances")]
public partial class Performance : CustomKubernetesEntity<PerformanceSpec, PerformanceStatus>;
