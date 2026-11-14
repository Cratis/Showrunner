// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import React, { useEffect, useState } from 'react';
import type { BoardTransform } from './tourGeometry.js';
import type { TourGesture } from './TourGesture.js';
import { TourAnchor } from './TourAnchor.js';
import { cursorSvg } from './cursorSvg.js';

/** How long the press ripple lasts. Must match the animation duration in the stylesheet. */
const PRESS_MS = 520;

/** How long the cursor takes to travel to whatever the narration has moved on to. */
const TRAVEL_MS = 850;

/** The pause between pressing down and starting to drag, so the press registers as its own beat. */
const DRAG_GRIP_MS = 250;

/** How long the held glide of a drag lasts before the pointer releases at its destination. */
const DRAG_MS = 1100;

export interface TourPointerProps {
    /** Where the pointer is going, or undefined before the tour has pointed at anything. */
    gesture: TourGesture | undefined;

    /** The board's live pan and zoom. */
    transformRef: React.RefObject<BoardTransform>;
}

/**
 * The tour's stand-in for a mouse: an operating-system-style arrow cursor — white with a black outline, so it
 * reads on any background — that glides to whatever the narration is talking about and presses when the
 * script says it does. The SVG's arrow tip sits exactly on the anchored point, and the press scales the
 * cursor down toward that tip, so what is being pressed never shifts.
 */
export const TourPointer = ({ gesture, transformRef }: TourPointerProps) => {
    const [isPressing, setIsPressing] = useState(false);
    const [dragPosition, setDragPosition] = useState<{ x: number; y: number } | null>(null);

    useEffect(() => {
        setDragPosition(null);

        if (gesture?.dragTo) {
            // A drag is press-and-hold: the cursor arrives, grips, glides to the destination while held,
            // and releases there — the same rhythm a real hand would have.
            const destination = gesture.dragTo;
            setIsPressing(false);
            const timers = [
                setTimeout(() => setIsPressing(true), TRAVEL_MS),
                setTimeout(() => setDragPosition(destination), TRAVEL_MS + DRAG_GRIP_MS),
                setTimeout(() => setIsPressing(false), TRAVEL_MS + DRAG_GRIP_MS + DRAG_MS),
            ];
            return () => timers.forEach(clearTimeout);
        }

        if (!gesture?.clicks) return;

        // The press lands once the cursor has arrived rather than the moment the step begins, so it reads as
        // the pointer pressing the thing it traveled to. Restarting the class rather than leaving it on is
        // what makes two presses in a row read as two presses instead of one long one.
        setIsPressing(false);
        const start = setTimeout(() => setIsPressing(true), TRAVEL_MS);
        const end = setTimeout(() => setIsPressing(false), TRAVEL_MS + PRESS_MS);
        return () => { clearTimeout(start); clearTimeout(end); };
    }, [gesture]);

    if (!gesture) return null;

    return (
        <TourAnchor
            x={dragPosition?.x ?? gesture.x}
            y={dragPosition?.y ?? gesture.y}
            space={gesture.space}
            transformRef={transformRef}
            glideMs={TRAVEL_MS}
            className='cue-pointer'>
            <div className={`cue-pointer__cursor${isPressing ? ' cue-pointer__cursor--pressing' : ''}`}>
                <span className='cue-pointer__ripple' />
                <span className='cue-pointer__arrow' dangerouslySetInnerHTML={{ __html: cursorSvg }} />
            </div>
        </TourAnchor>
    );
};
