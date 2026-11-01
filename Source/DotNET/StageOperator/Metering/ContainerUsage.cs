// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageOperator.Metering;

/// <summary>Represents the usage of one container at the moment metrics were read.</summary>
/// <param name="Pod">The pod.</param>
/// <param name="Container">The container.</param>
/// <param name="CpuCores">The cpu in use, in cores.</param>
/// <param name="MemoryBytes">The memory in use, in bytes.</param>
public record ContainerUsage(string Pod, string Container, double CpuCores, long MemoryBytes);
