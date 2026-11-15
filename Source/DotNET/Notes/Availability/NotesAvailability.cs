// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Arc.Queries.ModelBound;
using Cratis.Showrunner.Notes.Access;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Notes.Availability;

/// <summary>
/// Whether Notes is available to the user asking: switched on in the application and, when only granted users may give
/// feedback, granted to them. The feedback button renders only when it is.
/// </summary>
/// <param name="IsEnabled">Whether the host switched Notes on and the user may give feedback.</param>
[ReadModel]
public record NotesAvailability(bool IsEnabled)
{
    /// <summary>
    /// Gets whether Notes is available to the user asking. When everyone may give feedback it is the same answer for everyone
    /// and needs no sign-in: the button shows only to a signed-in user anyway, and the commands refuse everyone else. When only
    /// granted users may, a user nobody granted, or nobody signed in, is told it is not available, and learns nothing else.
    /// </summary>
    /// <param name="options">The <see cref="NotesOptions"/>.</param>
    /// <param name="access">Answers whether the user may give feedback.</param>
    /// <returns>Whether Notes is available.</returns>
    [AllowAnonymous]
    public static async Task<NotesAvailability> Availability(IOptions<NotesOptions> options, IFeedbackAccess access) =>
        new(options.Value.Enabled && await access.MayGiveFeedback());
}
