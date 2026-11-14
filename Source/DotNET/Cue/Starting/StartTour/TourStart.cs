// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Cue.Starting.StartTour;

/// <summary>
/// Who is starting a tour, as <see cref="StartTour"/> finds out before it decides.
/// </summary>
/// <param name="Viewer">The signed-in person.</param>
/// <param name="AlreadyStarted">Whether they had already started the tour.</param>
public record TourStart(ViewerSubject Viewer, bool AlreadyStarted);
