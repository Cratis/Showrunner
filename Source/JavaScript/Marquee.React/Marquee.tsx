// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { AnnouncementForMe } from '@cratis/showrunner.marquee';
import { ComponentType, useMemo } from 'react';
import { MarqueeBanner, MarqueeBannerProps } from './Banner/index.js';
import { MarqueeDialog, MarqueeDialogProps } from './Dialog/index.js';
import { MarqueeLabels, resolveLabels } from './Labels/index.js';
import { MarqueeMarkdown, MarqueeMarkdownProps } from './Markdown/index.js';
import { useAnnouncements } from './useAnnouncements.js';

/**
 * Props for {@link Marquee}.
 */
export interface MarqueeProps {
    /** Replaces the banners. Gets every banner to show, and draws them all. */
    banner?: ComponentType<MarqueeBannerProps>;

    /** Replaces the dialog. Gets one dialog at a time, the most important first. */
    dialog?: ComponentType<MarqueeDialogProps>;

    /** Replaces the Markdown renderer the default banner and dialog use, and that a custom one is given. */
    markdown?: ComponentType<MarqueeMarkdownProps>;

    /** The texts to replace; the rest stay English. */
    labels?: Partial<MarqueeLabels>;

    /** How often to apply the windows of the announcements against the clock, in milliseconds. Defaults to 60 seconds. */
    checkInterval?: number;
}

/**
 * Shows the announcements of a product the way its people expect: banners at the top, and dialogs one at a time. Mount
 * it once, first in the application shell and inside Arc's context; it brings its own queries and commands, so it needs
 * nothing but the backend package `Cratis.Showrunner.Marquee`.
 */
export const Marquee = ({ banner: Banner = MarqueeBanner, dialog: Dialog = MarqueeDialog, markdown = MarqueeMarkdown, labels, checkInterval }: MarqueeProps) => {
    const { banners, dialogs, close } = useAnnouncements({ checkInterval });
    const resolvedLabels = useMemo(() => resolveLabels(labels), [labels]);
    const [dialog, ...rest]: (AnnouncementForMe | undefined)[] = dialogs;

    return (
        <>
            <Banner announcements={banners} labels={resolvedLabels} markdown={markdown} />
            {dialog && (
                <Dialog
                    key={dialog.id.toString()}
                    announcement={dialog}
                    hasMore={rest.length > 0}
                    onClose={() => void close(dialog)}
                    labels={resolvedLabels}
                    markdown={markdown} />
            )}
        </>
    );
};
