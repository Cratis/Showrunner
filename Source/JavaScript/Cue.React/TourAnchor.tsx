// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import React, { useEffect, useLayoutEffect, useRef } from 'react';
import { canvasTransformActivity, createSelfSuspendingFrameLoop, type SelfSuspendingFrameLoop } from '@cratis/components/Canvas';
import type { TourAnchorSpace } from './TourAnchorSpace.js';
import { type BoardTransform, type ScreenPoint, projectEditorPointToScreen, projectToScreen, projectToolbarPointToScreen } from './tourGeometry.js';

/**
 * Where a target currently sits on screen, whatever space it is expressed in. Editor-space targets project
 * against the live viewport size, so the anchor keeps sitting on its spot in the panel through a resize.
 */
function projectTarget(target: { x: number; y: number; space: TourAnchorSpace }, transform: BoardTransform): ScreenPoint {
    switch (target.space) {
        case 'world': return projectToScreen(target.x, target.y, transform);
        case 'editor': return projectEditorPointToScreen(target.x, target.y, window.innerWidth, window.innerHeight);
        case 'toolbar': return projectToolbarPointToScreen(target.x, target.y, window.innerHeight);
        case 'screen': return { x: target.x, y: target.y };
    }
}

// How close, in screen pixels, a glide has to get before it counts as arrived and the loop suspends.
const GLIDE_SETTLE_DISTANCE = 0.5;

export interface TourAnchorProps {
    /** The horizontal coordinate to sit at, in the anchor's space. */
    x: number;

    /** The vertical coordinate to sit at, in the anchor's space. */
    y: number;

    /** The space the coordinates are expressed in. Board coordinates when omitted. */
    space?: TourAnchorSpace;

    /** The board's live pan and zoom. */
    transformRef: React.RefObject<BoardTransform>;

    /**
     * How long to take travelling to where the target currently is on screen. Left out, the anchor is simply
     * placed there every frame — which is what a label wants, where gliding would only look like the label
     * had come loose from the board.
     */
    glideMs?: number;

    /** A class applied to the anchored element. */
    className?: string;

    /** What to draw at that place. */
    children: React.ReactNode;
}

/**
 * Holds its children at a place on the board — or on the tour's own chrome — and, when asked, takes its time
 * getting to a new one.
 *
 * The travel is eased in screen space, against where the target currently sits on screen, rather than in
 * board space. This is what keeps the pointer's movement its own: while the camera pans, a board-space glide
 * would compose with the camera and sweep the pointer across the screen, whereas the screen-space easing lets
 * it drift calmly after its target no matter what the camera is doing underneath.
 *
 * The position is written straight to the DOM on every animation frame rather than held in React state: the
 * camera glides for the best part of a second on every step, and re-rendering the tour's overlay at frame
 * rate for the whole of it would compete with the board's own per-frame transform writes.
 */
export const TourAnchor = ({ x, y, space = 'world', transformRef, glideMs = 0, className, children }: TourAnchorProps) => {
    const anchorRef = useRef<HTMLDivElement>(null);
    const targetRef = useRef({ x, y, space });
    const currentRef = useRef<{ x: number; y: number } | null>(null);
    targetRef.current = { x, y, space };

    // The follower below only places the element from the first animation frame on — without this, the
    // element would be visible at the board's top-left corner for the frames before that, which is exactly
    // where a freshly appeared callout was seen flashing. Place it where it belongs before the first paint.
    useLayoutEffect(() => {
        const anchor = anchorRef.current;
        if (!anchor) return;

        const target = targetRef.current;
        const onScreen = projectTarget(target, transformRef.current);
        currentRef.current = onScreen;
        anchor.style.transform = `translate3d(${onScreen.x}px, ${onScreen.y}px, 0)`;
        // Mount-only by design: every later placement belongs to the follower.
    }, []);

    // The follower runs as a self-suspending loop rather than a permanent per-frame one: it glides while there is travel left, then stops, and wakes again whenever
    // the camera applies a transform frame, the window resizes (editor/toolbar spaces project against
    // the viewport), or the target itself moves to a new spot.
    const previousRef = useRef(performance.now());
    const step = (now: number): boolean => {
        const elapsed = now - previousRef.current;
        previousRef.current = now;

        const target = targetRef.current;
        const onScreen = projectTarget(target, transformRef.current);

        // Exponential easing toward the target's current screen position, so the travel is smooth at any
        // frame rate and settles rather than arriving abruptly. The first placement is not traveled to —
        // there is nowhere to have come from.
        const current = currentRef.current;
        let traveling = false;
        if (current === null || glideMs <= 0) {
            currentRef.current = onScreen;
        } else {
            const factor = 1 - Math.exp(-elapsed / (glideMs / 3));
            const next = {
                x: current.x + (onScreen.x - current.x) * factor,
                y: current.y + (onScreen.y - current.y) * factor,
            };
            // Within a hair of the target the glide has visually arrived — rest exactly on it so the
            // loop can suspend instead of easing asymptotically forever.
            traveling = Math.abs(next.x - onScreen.x) >= GLIDE_SETTLE_DISTANCE || Math.abs(next.y - onScreen.y) >= GLIDE_SETTLE_DISTANCE;
            currentRef.current = traveling ? next : onScreen;
        }

        const anchor = anchorRef.current;
        if (anchor) {
            anchor.style.transform = `translate3d(${currentRef.current.x}px, ${currentRef.current.y}px, 0)`;
        }

        return traveling;
    };
    const stepRef = useRef(step);
    stepRef.current = step;

    const loopRef = useRef<SelfSuspendingFrameLoop | null>(null);
    loopRef.current ??= createSelfSuspendingFrameLoop(now => stepRef.current(now));
    const loop = loopRef.current;

    useEffect(() => {
        const wake = () => {
            // A suspended loop's elapsed time restarts at its wake — measured from the frame it last
            // ran, the first glide step after a pause would leap most of the way in one jump.
            if (!loop.isRunning) {
                previousRef.current = performance.now();
            }
            loop.wake();
        };

        wake();
        const unsubscribe = canvasTransformActivity.subscribe(wake);
        window.addEventListener('resize', wake);
        return () => {
            unsubscribe();
            window.removeEventListener('resize', wake);
            loop.stop();
        };
    }, [loop]);

    // A new target (or glide behavior) has to restart a suspended glide.
    useEffect(() => {
        if (!loop.isRunning) {
            previousRef.current = performance.now();
        }
        loop.wake();
    }, [loop, x, y, space, glideMs]);

    return (
        <div ref={anchorRef} className={className} style={{ position: 'absolute', top: 0, left: 0, willChange: 'transform' }}>
            {children}
        </div>
    );
};
