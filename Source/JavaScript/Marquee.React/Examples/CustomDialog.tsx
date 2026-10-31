// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Marquee, MarqueeDialogProps } from '@cratis/showrunner.marquee.react';

const SheetDialog = ({ announcement, hasMore, onClose, labels, markdown: Markdown }: MarqueeDialogProps) => (
    <dialog open aria-label={announcement.title}>
        <h2>{announcement.title}</h2>
        {Markdown && <Markdown source={announcement.body} />}
        <button type='button' onClick={onClose}>{hasMore ? labels.next : labels.acknowledge}</button>
    </dialog>
);

export const MarqueeWithSheetDialog = () => <Marquee dialog={SheetDialog} />;
