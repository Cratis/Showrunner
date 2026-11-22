// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { DialogButtons } from '@cratis/arc.react/dialogs';
import { Dialog, DialogInitialFocus } from '@cratis/components/Dialogs';
import { ComponentType, useMemo, useState } from 'react';
import { usePlaybillClient } from '../Configuration/index.js';
import { PlaybillLabels, resolveLabels, withCount } from '../Labels/index.js';
import { PlaybillMarkdown, PlaybillMarkdownProps } from '../Markdown/index.js';
import { IssueRepository, issueRepositoryFor, ReleaseNoteView, ReleaseNoteViewProps } from '../Release/index.js';
import { usePlaybillStatus } from '../usePlaybillStatus.js';
import { useReleaseNotes } from '../useReleases.js';

/**
 * Props for {@link PlaybillDialog}.
 */
export interface PlaybillDialogProps {
    /** Replaces how one release is drawn. */
    release?: ComponentType<ReleaseNoteViewProps>;
    /** Replaces the Markdown renderer the default release view uses, and that a custom one is given. */
    markdown?: ComponentType<PlaybillMarkdownProps>;
    /** The texts to replace; the rest stay English. */
    labels?: Partial<PlaybillLabels>;
    /** The locale to write the dates in; the browser's when not given. */
    locale?: string | string[];
    /** Where the issues of the releases are tracked, so `#123` in a note links to one. A function answers for each version. Not linked when not given. */
    issues?: IssueRepository;
    /** The most releases to show at once, the newest first. Defaults to 5; the rest are pointed to with the `earlier` text. */
    maxReleases?: number;
}

/**
 * Tells the signed-in person what is new: a dialog that opens by itself with the notes of the releases they have not
 * seen, and records that they have when it is closed, so it does not return on any device. Nothing opens when nothing
 * is unread. Mount it once, in the application shell and inside Arc's context.
 */
export const PlaybillDialog = ({ release: Release = ReleaseNoteView, markdown, labels, locale, issues, maxReleases = 5 }: PlaybillDialogProps) => {
    const client = usePlaybillClient();
    const { unreadVersions, markSeen } = usePlaybillStatus();
    const resolved = useMemo(() => resolveLabels(labels), [labels]);
    const shown = useMemo(() => unreadVersions.slice(0, maxReleases), [unreadVersions, maxReleases]);
    const key = shown.join('|');
    const [dismissed, setDismissed] = useState<string>();
    const { data: releases, isLoading, error } = useReleaseNotes(shown);

    if (shown.length === 0 || dismissed === key) {
        return <></>;
    }

    const close = () => {
        setDismissed(key);
        // Failing to remember it must not keep a person from reading on.
        markSeen(shown[0]).catch(() => undefined);
    };

    return (
        <Dialog
            title={resolved.title}
            width='40rem'
            buttons={DialogButtons.Ok}
            okLabel={resolved.acknowledge}
            closeAriaLabel={resolved.close}
            initialFocus={DialogInitialFocus.Content}
            onConfirm={close}
            onCancel={close}>
            <div className='playbill-dialog'>
                {isLoading && <p className='playbill-status' role='status'>{resolved.loading}</p>}
                {error && <p className='playbill-status playbill-status--failed' role='alert'>{resolved.failed}</p>}
                {!isLoading && !error && releases?.map(note => (
                    <Release
                        key={note.version}
                        release={note}
                        assetBase={client.assetBase(note.version)}
                        labels={resolved}
                        markdown={markdown ?? PlaybillMarkdown}
                        locale={locale}
                        issueRepository={issueRepositoryFor(issues, note.version)}
                        headingLevel={3} />
                ))}
                {unreadVersions.length > shown.length && (
                    <p className='playbill-dialog__earlier'>{withCount(resolved.earlier, unreadVersions.length - shown.length)}</p>
                )}
            </div>
        </Dialog>
    );
};
