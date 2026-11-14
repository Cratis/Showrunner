// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Which coordinate space a tour anchor's point is expressed in.
 *
 * - `world` — board coordinates; the point stays glued to the board content as the camera moves.
 * - `screen` — pixels from the top-left of the board's box; the point ignores the camera entirely.
 *   Used for the tour's own chrome, such as the toolbar, which is not part of the board.
 * - `editor` — pixels from the center of the editor showcase's panel; the point sits on the same spot inside
 *   the panel on every viewport, because the panel itself is pinned on the unobstructed center.
 * - `toolbar` — pixels from the board's left edge across, and from the viewport's vertical center down; the
 *   point sits on the same spot of the vertically-centered toolbar rail on every viewport.
 */
export type TourAnchorSpace = 'world' | 'screen' | 'editor' | 'toolbar';
