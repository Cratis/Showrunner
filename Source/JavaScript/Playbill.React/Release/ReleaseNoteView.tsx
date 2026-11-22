// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ComponentType } from 'react';
import { Release } from '../Client/index.js';
import { PlaybillLabels } from '../Labels/index.js';
import { linkIssueReferences, PlaybillMarkdown, PlaybillMarkdownProps } from '../Markdown/index.js';
import { formatReleaseDate } from './formatReleaseDate.js';
import { sectionPresentation } from './sectionPresentation.js';

/**
 * Props for {@link ReleaseNoteView}, and for the component given to the `release` slot of {@link Playbill} and
 * {@link PlaybillDialog}.
 */
export interface ReleaseNoteViewProps {
    /** The release to show. */
    release: Release;
    /** Where the files of the release are served from; given to the Markdown renderer so its images load. */
    assetBase?: string;
    /** The texts to show. */
    labels: PlaybillLabels;
    /** Renders the Markdown of the note; the `markdown` slot of {@link Playbill}. */
    markdown?: ComponentType<PlaybillMarkdownProps>;
    /** The locale to write the date in; the browser's when not given. */
    locale?: string | string[];
    /** Whether the person has not seen this release yet; it is marked with the `unread` text. */
    isUnread?: boolean;
    /** Whether this is the version the application is running; it is marked with the `current` text. */
    isCurrent?: boolean;
    /** The address of the repository whose issues a bare `#123` in the note refers to. The references are made links when it is given. */
    issueRepository?: string;
    /** The level of the heading of the version, so the release fits under the heading of the page or dialog around it. Defaults to 2. */
    headingLevel?: 2 | 3 | 4;
}

/**
 * One release: its version and date, what its note says before the first section, and each section under its heading.
 * It draws no frame of its own, so a page, a dialog or a product's own surface can hold it.
 */
export const ReleaseNoteView = ({
    release,
    assetBase,
    labels,
    markdown: Markdown = PlaybillMarkdown,
    locale,
    isUnread = false,
    isCurrent = false,
    issueRepository,
    headingLevel = 2
}: ReleaseNoteViewProps) => {
    const Heading = `h${headingLevel}` as 'h2';
    const SectionHeading = `h${headingLevel + 1}` as 'h3';
    const date = formatReleaseDate(release.releasedAt, locale);
    const prepared = (markdown: string) => issueRepository ? linkIssueReferences(markdown, issueRepository) : markdown;
    return (
        <article className='playbill-release' data-version={release.version}>
            <header className='playbill-release__header'>
                <Heading className='playbill-release__version'>{release.version}</Heading>
                {isCurrent && <span className='playbill-tag playbill-tag--current'>{labels.current}</span>}
                {isUnread && <span className='playbill-tag playbill-tag--unread'>{labels.unread}</span>}
                {date && (
                    <time className='playbill-release__date' dateTime={release.releasedAt ?? undefined}>
                        {labels.released} {date}
                    </time>
                )}
            </header>
            {release.lead.trim() !== '' && (
                <div className='playbill-release__lead'>
                    <Markdown source={prepared(release.lead)} assetBase={assetBase} />
                </div>
            )}
            {release.sections.map(section => {
                const presentation = sectionPresentation(section.name, labels);
                return (
                    <section key={section.name} className={`playbill-section playbill-section--${presentation.modifier}`}>
                        <SectionHeading className='playbill-section__title'>{presentation.title}</SectionHeading>
                        <Markdown source={prepared(section.markdown)} assetBase={assetBase} />
                    </section>
                );
            })}
        </article>
    );
};
