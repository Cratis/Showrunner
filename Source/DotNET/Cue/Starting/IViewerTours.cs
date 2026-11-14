// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Cue.Starting;

/// <summary>
/// Defines what each person has started.
/// </summary>
public interface IViewerTours
{
    /// <summary>
    /// Gets the tours a person started.
    /// </summary>
    /// <param name="viewer">The person.</param>
    /// <returns>What they started; nothing when they never started a tour.</returns>
    Task<StartedTours> Of(ViewerSubject viewer);
}
