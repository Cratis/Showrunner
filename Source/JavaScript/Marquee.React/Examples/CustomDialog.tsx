// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { hasMarkdown, markdownToPlainText, Marquee, MarqueeDialogProps, MarqueeInlineMarkdown } from '@cratis/showrunner.marquee.react';

const SheetDialog = ({ announcement, hasMore, onClose, labels, markdown: Markdown }: MarqueeDialogProps) => (
    <dialog open aria-label={markdownToPlainText(announcement.title)}>
        <h2><MarqueeInlineMarkdown source={announcement.title} /></h2>
        {Markdown && hasMarkdown(announcement.body) && <Markdown source={announcement.body} />}
        <button type='button' onClick={onClose}>{hasMore ? labels.next : labels.acknowledge}</button>
    </dialog>
);

export const MarqueeWithSheetDialog = () => <Marquee dialog={SheetDialog} />;
