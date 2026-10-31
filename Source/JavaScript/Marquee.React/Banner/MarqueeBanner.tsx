// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { AnnouncementForMe } from '@cratis/showrunner.marquee';
import { ComponentType } from 'react';
import { ImportanceIcon } from '../Icons.js';
import { MarqueeLabels } from '../Labels/index.js';
import { MarqueeMarkdown, MarqueeMarkdownProps } from '../Markdown/index.js';
import { importancePresentation } from '../Selection/index.js';

/**
 * Props for {@link MarqueeBanner}, and for the component given to the `banner` slot of {@link Marquee}.
 */
export interface MarqueeBannerProps {
    /** The banners to show, the most important first. Empty when there is none. */
    announcements: AnnouncementForMe[];

    /** The texts to show. */
    labels: MarqueeLabels;

    /** Renders the Markdown of a body; the `markdown` slot of {@link Marquee}. */
    markdown?: ComponentType<MarqueeMarkdownProps>;
}

/**
 * The banners at the top of the application, one row for each, with the icon and the color of its importance.
 *
 * The region is announced politely when a row appears, and is always rendered, because a live region that is added
 * with its content is not reliably announced. It takes no space while it is empty. A banner has no close button: it
 * stays until it is withdrawn or its window ends.
 */
export const MarqueeBanner = ({ announcements, labels, markdown: Markdown = MarqueeMarkdown }: MarqueeBannerProps) => (
    <div className='marquee-banner' role='status' aria-live='polite' aria-label={labels.banners}>
        {announcements.map(announcement => {
            const presentation = importancePresentation(announcement.importance, labels);
            return (
                <div key={announcement.id.toString()} className={`marquee-banner__item marquee-banner__item--${presentation.modifier}`}>
                    <ImportanceIcon presentation={presentation} />
                    <div className='marquee-banner__text'>
                        <span className='marquee-banner__title'>{announcement.title}</span>
                        <div className='marquee-banner__body'>
                            <Markdown source={announcement.body} compact />
                        </div>
                    </div>
                </div>
            );
        })}
    </div>
);
