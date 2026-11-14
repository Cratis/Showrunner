// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Cue.Starting;

/// <summary>
/// A person started a guided tour. Appended under the person's <see cref="ViewerId"/>, with the person as compliance
/// subject. Only the first start of a tour is a fact: starting it again records nothing.
/// </summary>
/// <param name="Tour">The tour.</param>
/// <param name="Viewer">Who started it.</param>
[EventType]
public record TourStarted(TourId Tour, ViewerSubject Viewer);
