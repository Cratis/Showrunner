// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ComponentType, useEffect, useMemo, useState } from 'react';
import { usePlaybillClient } from './Configuration/index.js';
import { PlaybillLabels, resolveLabels } from './Labels/index.js';
import { PlaybillMarkdown, PlaybillMarkdownProps } from './Markdown/index.js';
import { formatReleaseDate, IssueRepository, issueRepositoryFor, ReleaseNoteView, ReleaseNoteViewProps } from './Release/index.js';
import { usePlaybill } from './usePlaybill.js';
import { useRelease } from './useReleases.js';

/**
 * Props for {@link Playbill}.
 */
export interface PlaybillProps {
    /** Replaces how the selected release is drawn. */
    release?: ComponentType<ReleaseNoteViewProps>;
    /** Replaces the Markdown renderer the default release view uses, and that a custom one is given. */
    markdown?: ComponentType<PlaybillMarkdownProps>;
    /** The texts to replace; the rest stay English. */
    labels?: Partial<PlaybillLabels>;
    /** The locale to write the dates in; the browser's when not given. */
    locale?: string | string[];
    /** Where the issues of the releases are tracked, so `#123` in a note links to one. A function answers for each version. Not linked when not given. */
    issues?: IssueRepository;
    /** The version to show. Give it, with `onSelect`, to keep the selection in the address; by default the newest is selected. */
    selected?: string;
    /** Called when the person picks another release. */
    onSelect?: (version: string) => void;
    /** Records that the person has seen a release when it is shown. On by default; turn it off to leave what is unread to a dialog. */
    markSeenWhenShown?: boolean;
}

/**
 * The release notes of the application as a page: the releases newest first on one side and the selected release on the
 * other. It marks what the person has not seen with a tag, and records a release as seen when it is shown. Place it
 * where the application has room for a page, inside Arc's context. It draws no heading of its own.
 */
export const Playbill = ({
    release: Release = ReleaseNoteView,
    markdown,
    labels,
    locale,
    issues,
    selected,
    onSelect,
    markSeenWhenShown = true
}: PlaybillProps) => {
    const client = usePlaybillClient();
    const { releases, current, isLoading, error, unreadVersions, markSeen } = usePlaybill();
    const resolved = useMemo(() => resolveLabels(labels), [labels]);
    const [picked, setPicked] = useState<string>();
    const version = selected ?? picked ?? releases[0]?.version;
    const { data: note, isLoading: isLoadingNote, error: noteError } = useRelease(version);
    const isUnread = (candidate?: string) => candidate !== undefined && unreadVersions.includes(candidate);
    const selectedIsUnread = isUnread(version);

    useEffect(() => {
        if (markSeenWhenShown && note && selectedIsUnread) {
            // Failing to remember it must not keep a person from reading on.
            markSeen(note.version).catch(() => undefined);
        }
    }, [markSeenWhenShown, note, selectedIsUnread, markSeen]);

    const select = (next: string) => {
        setPicked(next);
        onSelect?.(next);
    };

    if (error || noteError) {
        return <p className='playbill-status playbill-status--failed' role='alert'>{resolved.failed}</p>;
    }

    if (isLoading) {
        return <p className='playbill-status' role='status'>{resolved.loading}</p>;
    }

    if (releases.length === 0) {
        return <p className='playbill-status'>{resolved.empty}</p>;
    }

    return (
        <div className='playbill'>
            <nav className='playbill__list' aria-label={resolved.releases}>
                <ul>
                    {releases.map(summary => {
                        const date = formatReleaseDate(summary.releasedAt, locale);
                        const isSelected = summary.version === version;
                        return (
                            <li key={summary.version}>
                                <button
                                    type='button'
                                    className={isSelected ? 'playbill-item playbill-item--selected' : 'playbill-item'}
                                    aria-current={isSelected ? 'true' : undefined}
                                    onClick={() => select(summary.version)}>
                                    <span className='playbill-item__heading'>
                                        <span className='playbill-item__version'>{summary.version}</span>
                                        {summary.version === current && <span className='playbill-tag playbill-tag--current'>{resolved.current}</span>}
                                        {isUnread(summary.version) && <span className='playbill-tag playbill-tag--unread'>{resolved.unread}</span>}
                                    </span>
                                    {date && <span className='playbill-item__date'>{date}</span>}
                                    {summary.summary && <span className='playbill-item__summary'>{summary.summary}</span>}
                                </button>
                            </li>
                        );
                    })}
                </ul>
            </nav>
            <div className='playbill__content'>
                {isLoadingNote && <p className='playbill-status' role='status'>{resolved.loading}</p>}
                {!isLoadingNote && note && (
                    <Release
                        release={note}
                        assetBase={client.assetBase(note.version)}
                        labels={resolved}
                        markdown={markdown ?? PlaybillMarkdown}
                        locale={locale}
                        issueRepository={issueRepositoryFor(issues, note.version)}
                        isUnread={selectedIsUnread}
                        isCurrent={note.version === current} />
                )}
            </div>
        </div>
    );
};
