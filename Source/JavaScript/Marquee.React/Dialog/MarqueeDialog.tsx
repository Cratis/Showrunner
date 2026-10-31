// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { DialogButtons } from '@cratis/arc.react/dialogs';
import { Dialog, DialogInitialFocus } from '@cratis/components/Dialogs';
import { AnnouncementForMe } from '@cratis/showrunner.marquee';
import { ComponentType } from 'react';
import { ImportanceIcon } from '../Icons.js';
import { MarqueeLabels } from '../Labels/index.js';
import { MarqueeMarkdown, MarqueeMarkdownProps } from '../Markdown/index.js';
import { importancePresentation } from '../Selection/index.js';

/**
 * Props for {@link MarqueeDialog}, and for the component given to the `dialog` slot of {@link Marquee}.
 */
export interface MarqueeDialogProps {
    /** The announcement to show. */
    announcement: AnnouncementForMe;

    /** Whether more dialogs follow this one; the button then moves on instead of finishing. */
    hasMore: boolean;

    /** Called when the person closes the dialog, with the button, Escape or the close button of the header. */
    onClose: () => void;

    /** The texts to show. */
    labels: MarqueeLabels;

    /** Renders the Markdown of a body; the `markdown` slot of {@link Marquee}. */
    markdown?: ComponentType<MarqueeMarkdownProps>;
}

/**
 * One announcement in a dialog. Focus moves into the dialog and is kept in it until it is closed, and lands on the
 * title instead of the button, so the Enter or Space that was being pressed when it opened does not close it unread.
 */
export const MarqueeDialog = ({ announcement, hasMore, onClose, labels, markdown: Markdown = MarqueeMarkdown }: MarqueeDialogProps) => {
    const presentation = importancePresentation(announcement.importance, labels);

    return (
        <Dialog
            title={announcement.title}
            width='34rem'
            buttons={DialogButtons.Ok}
            okLabel={hasMore ? labels.next : labels.acknowledge}
            closeAriaLabel={labels.close}
            initialFocus={DialogInitialFocus.Content}
            onConfirm={onClose}
            onCancel={onClose}>
            <div className='marquee-dialog'>
                <div className={`marquee-dialog__importance marquee-dialog__importance--${presentation.modifier}`}>
                    <ImportanceIcon presentation={presentation} />
                </div>
                <Markdown source={announcement.body} />
            </div>
        </Dialog>
    );
};
