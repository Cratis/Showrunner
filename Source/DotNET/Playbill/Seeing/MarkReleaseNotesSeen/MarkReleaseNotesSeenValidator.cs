// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Playbill.Seeing.MarkReleaseNotesSeen;

/// <summary>
/// Validates <see cref="MarkReleaseNotesSeen"/>.
/// </summary>
public class MarkReleaseNotesSeenValidator : CommandValidator<MarkReleaseNotesSeen>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MarkReleaseNotesSeenValidator"/> class.
    /// </summary>
    public MarkReleaseNotesSeenValidator() =>
        RuleFor(_ => _.Version)
            .Must(version => new ReleaseVersion(version).IsValid)
            .WithMessage("The version must be a version number, such as 2.3.0.");
}
