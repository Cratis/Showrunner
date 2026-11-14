// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useCueConfiguration } from './Configuration/CueProvider.js';
import type { TimedTourChapter, TimedTourSection } from './tourTimeline.js';
import { formatTourTime } from './tourTimeline.js';

export interface TourChapterListProps {
    /** The sections, already placed on the tour's clock. */
    sections: TimedTourSection[];

    /** The chapters across all sections in play order, already placed on the tour's clock. */
    chapters: TimedTourChapter[];

    /** Which chapter is playing, as an index into {@link chapters}. */
    currentChapterIndex: number;

    /** Jumps to the start of a chapter, by its index into {@link chapters}. */
    onSelectChapter: (chapterIndex: number) => void;

    /**
     * Whether to show where on the clock each chapter starts. On by default; self-paced reading switches it
     * off, because there is no clock to time against when nobody is playing.
     */
    showTimings?: boolean;
}

/**
 * The tour's table of contents: the sections as headings, their chapters under them, where the viewer is in
 * it all, and a way to skip straight to any part of it. Chapter indices stay global across sections — the
 * clock is flat, sections only group the listing.
 */
export const TourChapterList = ({ sections, chapters, currentChapterIndex, onSelectChapter, showTimings = true }: TourChapterListProps) => {
    const { labels } = useCueConfiguration();
    return (
        <aside className='cue-chapters'>
            <h2 className='cue-chapters__heading'>{labels.chapters}</h2>
            <div className='cue-chapters__sections'>
                {sections.map((timedSection, sectionIndex) => (
                    <section key={timedSection.section.id}>
                        <h3 className='cue-chapters__section-title'>{timedSection.section.title}</h3>
                        <ol className='cue-chapters__list'>
                            {chapters.map((timed, chapterIndex) => ({ timed, chapterIndex }))
                                .filter(({ timed }) => timed.sectionIndex === sectionIndex)
                                .map(({ timed, chapterIndex }) => (
                                    <li key={timed.chapter.id}>
                                        <button
                                            type='button'
                                            className={`cue-chapters__item${chapterIndex === currentChapterIndex ? ' cue-chapters__item--current' : ''}`}
                                            onClick={() => onSelectChapter(chapterIndex)}>
                                            {showTimings && <span className='cue-chapters__time'>{formatTourTime(timed.startMs)}</span>}
                                            <span className='cue-chapters__text'>
                                                <span className='cue-chapters__title'>{timed.chapter.title}</span>
                                                <span className='cue-chapters__summary'>{timed.chapter.summary}</span>
                                            </span>
                                        </button>
                                    </li>
                                ))}
                        </ol>
                    </section>
                ))}
            </div>
        </aside>
    );
};
