// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/** A point on the screen, relative to the board's own box. */
export interface ScreenPoint {
    /** The horizontal offset in pixels. */
    x: number;

    /** The vertical offset in pixels. */
    y: number;
}

/** The board's current pan and zoom, as the canvas reports it. */
export interface BoardTransform {
    /** The zoom level, where 1 is actual size. */
    zoom: number;

    /** The pan offset in pixels. */
    pan: ScreenPoint;
}

/**
 * How much of the viewport the tour's own chrome covers on each side, in pixels. The camera compensates by
 * this much so a framed subject centers in the *unobstructed* part of the viewport instead of behind the
 * chapter list or the transport, and the editor showcase centers its panel on the same point. Must be kept in
 * sync with the chrome's stylesheets: the right inset is the chapter list (1rem margin + 19rem wide), the
 * left inset is the tour toolbar's column, and the bottom inset is the subtitles' box (bottom 5.5rem plus its
 * height) over the transport bar.
 */
export const CHROME_INSETS = { left: 72, right: 320, bottom: 136 } as const;

/**
 * Projects a board coordinate to where it currently sits on screen.
 * @param worldX The horizontal board coordinate.
 * @param worldY The vertical board coordinate.
 * @param transform The board's current pan and zoom.
 * @returns The {@link ScreenPoint} relative to the board's box.
 * @remarks
 * This is the exact inverse of the screen-to-board mapping the canvas uses for collaborator presence, and it
 * is what keeps the tour's pointer and callouts glued to the thing they are pointing at while the camera
 * moves underneath them.
 */
export function projectToScreen(worldX: number, worldY: number, transform: BoardTransform): ScreenPoint {
    return {
        x: transform.pan.x + worldX * transform.zoom,
        y: transform.pan.y + worldY * transform.zoom,
    };
}

/**
 * The point the viewport's unobstructed part centers on — the middle of what the tour's chrome leaves open.
 * @param viewportWidth The viewport's width in pixels.
 * @param viewportHeight The viewport's height in pixels.
 * @returns The {@link ScreenPoint} the camera aims subjects at and the editor showcase centers its panel on.
 */
export function unobstructedCenter(viewportWidth: number, viewportHeight: number): ScreenPoint {
    return {
        x: viewportWidth / 2 - (CHROME_INSETS.right - CHROME_INSETS.left) / 2,
        y: viewportHeight / 2 - CHROME_INSETS.bottom / 2,
    };
}

/**
 * Projects an editor-space point — an offset from the center of the editor showcase's panel — to the screen.
 * @param x The horizontal offset from the panel's center.
 * @param y The vertical offset from the panel's center.
 * @param viewportWidth The viewport's width in pixels.
 * @param viewportHeight The viewport's height in pixels.
 * @returns The {@link ScreenPoint} relative to the board's box.
 * @remarks
 * The showcase pins its panel on the unobstructed center whatever the viewport size, so offsets from that
 * center are the one coordinate system in which a spot inside the panel — a button, a field, a tab — has the
 * same coordinates on every screen. Measured offsets therefore stay valid everywhere, exactly like the
 * toolbar's fixed screen coordinates.
 */
export function projectEditorPointToScreen(x: number, y: number, viewportWidth: number, viewportHeight: number): ScreenPoint {
    const center = unobstructedCenter(viewportWidth, viewportHeight);
    return { x: center.x + x, y: center.y + y };
}

/**
 * Projects a toolbar-space point — a fixed offset from the left edge, and a vertical offset from the
 * viewport's center — to the screen.
 * @param x The horizontal offset from the board's left edge.
 * @param y The vertical offset from the viewport's vertical center.
 * @param viewportHeight The viewport's height in pixels.
 * @returns The {@link ScreenPoint} relative to the board's box.
 * @remarks
 * The tour's toolbar centers itself vertically the way the editor's own toolbar does, so a spot on one of
 * its buttons is a fixed distance from the vertical center on every viewport — this is the coordinate system
 * in which those spots keep the same plain numbers everywhere.
 */
export function projectToolbarPointToScreen(x: number, y: number, viewportHeight: number): ScreenPoint {
    return { x, y: viewportHeight / 2 + y };
}
