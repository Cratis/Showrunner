// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { AnnouncementForMe } from '@cratis/showrunner.marquee';
import { ComponentType, KeyboardEvent, useId, useState } from 'react';
import { ChevronIcon, ImportanceIcon } from '../Icons.js';
import { MarqueeLabels } from '../Labels/index.js';
import { hasMarkdown, MarqueeInlineMarkdown, MarqueeMarkdown, MarqueeMarkdownProps } from '../Markdown/index.js';
import { importancePresentation } from '../Selection/index.js';

/**
 * Props for {@link MarqueeBanner}, and for the component given to the `banner` slot of {@link Marquee}.
 */
export interface MarqueeBannerProps {
    /** The banners to show, the most important first. Empty when there is none. */
    announcements: AnnouncementForMe[];

    /** The texts to show. */
    labels: MarqueeLabels;

    /** Renders the Markdown of a message; the `markdown` slot of {@link Marquee}. */
    markdown?: ComponentType<MarqueeMarkdownProps>;
}

interface MarqueeBannerItemProps {
    announcement: AnnouncementForMe;
    labels: MarqueeLabels;
    markdown: ComponentType<MarqueeMarkdownProps>;
}

const MarqueeBannerItem = ({ announcement, labels, markdown: Markdown }: MarqueeBannerItemProps) => {
    const presentation = importancePresentation(announcement.importance, labels);
    const identifier = useId();
    const messageId = `${identifier}-message`;
    const tooltipId = `${identifier}-tooltip`;
    const [expanded, setExpanded] = useState(false);
    const [tooltipVisible, setTooltipVisible] = useState(false);
    const hasMessage = hasMarkdown(announcement.body);

    const handleKeyDown = (event: KeyboardEvent) => {
        if (event.key === 'Escape') {
            setTooltipVisible(false);
        }
    };

    return (
        <div className={`marquee-banner__item marquee-banner__item--${presentation.modifier}`}>
            <div className='marquee-banner__row'>
                <ImportanceIcon presentation={presentation} />
                <span className='marquee-banner__title'>
                    <MarqueeInlineMarkdown source={announcement.title} />
                </span>
                {hasMessage && (
                    <span
                        className='marquee-banner__more'
                        onMouseEnter={() => setTooltipVisible(true)}
                        onMouseLeave={() => setTooltipVisible(false)}
                        onFocus={() => setTooltipVisible(true)}
                        onBlur={() => setTooltipVisible(false)}
                        onKeyDown={handleKeyDown}>
                        <button
                            type='button'
                            className='marquee-banner__toggle'
                            aria-expanded={expanded}
                            aria-controls={messageId}
                            aria-describedby={tooltipId}
                            aria-label={expanded ? labels.collapse : labels.expand}
                            onClick={() => setExpanded(!expanded)}>
                            <ChevronIcon expanded={expanded} />
                        </button>
                        <span
                            id={tooltipId}
                            role='tooltip'
                            className={tooltipVisible && !expanded ? 'marquee-banner__tooltip marquee-banner__tooltip--visible' : 'marquee-banner__tooltip'}>
                            <Markdown source={announcement.body} compact />
                        </span>
                    </span>
                )}
            </div>
            {hasMessage && (
                <div id={messageId} className='marquee-banner__message' hidden={!expanded}>
                    <Markdown source={announcement.body} />
                </div>
            )}
        </div>
    );
};

/**
 * The banners at the top of the application, one row for each, with the icon and the color of its importance.
 *
 * A banner shows its title only, with inline Markdown such as a link. When the announcement has a message, the row ends
 * in a button that shows the message inline below the title, or hides it again, and hovering or focusing the button
 * shows the message in a tooltip. Without a message there is no button and no tooltip.
 *
 * The region is announced politely when a row appears, and is always rendered, because a live region that is added
 * with its content is not reliably announced. It takes no space while it is empty. A banner has no close button: it
 * stays until it is withdrawn or its window ends.
 */
export const MarqueeBanner = ({ announcements, labels, markdown = MarqueeMarkdown }: MarqueeBannerProps) => (
    <div className='marquee-banner' role='status' aria-live='polite' aria-label={labels.banners}>
        {announcements.map(announcement => (
            <MarqueeBannerItem key={announcement.id.toString()} announcement={announcement} labels={labels} markdown={markdown} />
        ))}
    </div>
);
