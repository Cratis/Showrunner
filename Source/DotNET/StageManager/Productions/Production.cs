// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// Chronicle, with its database and an optional Neo4j graph, for one or more applications. Reconciled by <see cref="ProductionController"/>.
/// </summary>
[KubernetesEntity(Group = StageManagerApi.Group, ApiVersion = StageManagerApi.Version, Kind = "Production", PluralName = "productions")]
public partial class Production : CustomKubernetesEntity<ProductionSpec, ProductionStatus>;
