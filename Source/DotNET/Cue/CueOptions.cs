// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Cue;

/// <summary>
/// Represents how an application configures Cue.
/// </summary>
public class CueOptions
{
    /// <summary>
    /// Gets or sets the names of the guided tours the application has. Starting a tour that is not among them is refused, so
    /// a client cannot fill a person's history with tours that do not exist. Left empty, any tour name is accepted.
    /// </summary>
    public IList<string> Tours { get; set; } = [];

    /// <summary>
    /// Gets whether a tour is one the application has.
    /// </summary>
    /// <param name="tour">The tour to check.</param>
    /// <returns><see langword="true"/> when <see cref="Tours"/> is empty, or names the tour; otherwise <see langword="false"/>.</returns>
    public bool Has(TourId tour) => Tours.Count == 0 || Tours.Contains(tour.Value);
}
