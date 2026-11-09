// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// What a Production needs to be applied, once every input it names is known to be there.
/// </summary>
/// <param name="Kernel">The kernel configuration of Chronicle.</param>
public record PreparedProduction(ChronicleKernelConfiguration Kernel);
