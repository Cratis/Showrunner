// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { CueMarkdown } from './Markdown/CueMarkdown.js';

export interface TourSelfPacedPanelProps {
    /** The title of the chapter being read. */
    title: string;

    /** The chapter's markdown walkthrough. */
    markdown: string;
}

/**
 * The self-paced reading panel: the current chapter's written walkthrough, docked beside the chapter list so
 * reading and navigating sit next to each other. It renders the tour's own authored markdown — screenshots
 * and animations included — and scrolls on its own, since a written chapter is longer than its narration.
 */
export const TourSelfPacedPanel = ({ title, markdown }: TourSelfPacedPanelProps) => (
    <aside className='cue-self-paced'>
        <h2 className='cue-self-paced__title'>{title}</h2>
        <div className='cue-self-paced__content'>
            <CueMarkdown source={markdown} />
        </div>
    </aside>
);
