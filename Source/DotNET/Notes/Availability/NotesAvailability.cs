// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Arc.Queries.ModelBound;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Notes.Availability;

/// <summary>
/// Whether Notes is switched on in the application. The feedback button renders only when it is.
/// </summary>
/// <param name="IsEnabled">Whether the host switched Notes on.</param>
[ReadModel]
public record NotesAvailability(bool IsEnabled)
{
    /// <summary>
    /// Gets whether Notes is switched on. It is the same answer for everyone, so it needs no sign-in: the button shows only
    /// to a signed-in user anyway, and the commands refuse everyone else.
    /// </summary>
    /// <param name="options">The <see cref="NotesOptions"/>.</param>
    /// <returns>Whether Notes is on.</returns>
    [AllowAnonymous]
    public static NotesAvailability Availability(IOptions<NotesOptions> options) => new(options.Value.Enabled);
}
