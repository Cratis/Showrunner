// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import React from 'react';
import type { BoardTransform } from './tourGeometry.js';
import type { TourCallout } from './TourCallout.js';
import { TourAnchor } from './TourAnchor.js';

export interface TourCalloutBubbleProps {
    /** The callout to show, or undefined when the step has nothing to point out. */
    callout: TourCallout | undefined;

    /** The board's live pan and zoom. */
    transformRef: React.RefObject<BoardTransform>;
}

/**
 * Names the thing the narration is talking about, pinned just above it on the board with its caret pointing
 * down at it. The script is responsible for anchoring callouts above their subjects — the bubble always
 * renders upward from its anchor.
 */
export const TourCalloutBubble = ({ callout, transformRef }: TourCalloutBubbleProps) => {
    if (!callout) return null;

    return (
        <TourAnchor
            // Keyed by its text so a new callout fades in as itself rather than the previous one's words
            // being swapped out underneath a bubble that never moved.
            key={callout.text}
            x={callout.x}
            y={callout.y}
            space={callout.space}
            transformRef={transformRef}
            className='cue-callout'>
            <div className='cue-callout__bubble'>{callout.text}</div>
        </TourAnchor>
    );
};
