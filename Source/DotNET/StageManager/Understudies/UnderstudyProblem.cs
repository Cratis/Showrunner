// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Understudies;

/// <summary>
/// Something wrong with the spec of an <see cref="Understudy"/>.
/// </summary>
/// <param name="Reason">A short reason in CamelCase.</param>
/// <param name="Message">What is wrong. It never contains a secret value.</param>
public record UnderstudyProblem(string Reason, string Message);
