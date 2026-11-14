// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Arc.Validation;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Cue.Starting.StartTour;

/// <summary>
/// Validates <see cref="StartTour"/>.
/// </summary>
public class StartTourValidator : CommandValidator<StartTour>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StartTourValidator"/> class.
    /// </summary>
    /// <param name="options">The <see cref="CueOptions"/>, naming the tours the application has.</param>
    public StartTourValidator(IOptions<CueOptions> options) =>
        RuleFor(_ => _.Tour)
            .Must(tour => !string.IsNullOrWhiteSpace(tour) && options.Value.Has(tour))
            .WithMessage("The tour must be one of the guided tours the application has.");
}
