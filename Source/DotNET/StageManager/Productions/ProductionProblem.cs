// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// One thing wrong with the spec of a <see cref="Production"/>.
/// </summary>
/// <param name="Message">What is wrong. It never holds a secret value.</param>
/// <param name="Sink">The named sink the problem is about, or null when it is about the Production as a whole.</param>
public record ProductionProblem(string Message, string? Sink = null);
