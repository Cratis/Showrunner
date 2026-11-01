// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageOperator.Reconciling;

/// <summary>Represents why a resource is refused.</summary>
/// <param name="Reason">The machine readable reason.</param>
/// <param name="Message">The human readable message.</param>
public record SpecProblem(string Reason, string Message);
