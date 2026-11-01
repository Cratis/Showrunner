// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageOperator.Reconciling;

/// <summary>Represents what the cluster holds for an application right now.</summary>
/// <param name="Deployment">The Deployment, or null when it does not exist.</param>
/// <param name="DataExists">Whether the MongoDB StatefulSet or its volume exists.</param>
public record ObservedState(ObservedDeployment? Deployment, bool DataExists);
