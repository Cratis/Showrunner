// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// Chronicle plus an optional MongoDB and Vault, for one or more applications. Schema only until the Production reconciliation lands.
/// </summary>
[KubernetesEntity(Group = StageManagerApi.Group, ApiVersion = StageManagerApi.Version, Kind = "Production", PluralName = "productions")]
public partial class Production : CustomKubernetesEntity<ProductionSpec, ProductionStatus>;
