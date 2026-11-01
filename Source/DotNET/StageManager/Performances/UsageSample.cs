// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// What the pods of a Performance use at one moment, summed over all their containers.
/// </summary>
/// <param name="CpuCores">The CPU in use, in cores.</param>
/// <param name="MemoryBytes">The memory in use, in bytes.</param>
public record UsageSample(double CpuCores, double MemoryBytes);
