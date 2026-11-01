// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Decides whether the Notes components render anything: only for a signed-in user, and only while Notes is switched
 * on.
 * @param isSignedIn Whether the user is signed in.
 * @param isEnabled Whether Notes is switched on, as `NotesAvailability` tells; false until it is known.
 * @returns True when the Notes components render.
 */
export function isNotesAvailable(isSignedIn: boolean, isEnabled: boolean | undefined): boolean {
    return isSignedIn && isEnabled === true;
}
