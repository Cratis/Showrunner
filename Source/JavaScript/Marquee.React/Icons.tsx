// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ImportancePresentation } from './Selection/index.js';

const paths: Record<ImportancePresentation['modifier'], string> = {
    information: 'M12 2a10 10 0 1 0 0 20 10 10 0 0 0 0-20zm1 15h-2v-6h2v6zm0-8h-2V7h2v2z',
    warning: 'M1 21h22L12 2 1 21zm12-3h-2v-2h2v2zm0-4h-2v-4h2v4z',
    critical: 'M12 2a10 10 0 1 0 0 20 10 10 0 0 0 0-20zm1 15h-2v-2h2v2zm0-4h-2V7h2v6z'
};

/**
 * Props for {@link ChevronIcon}.
 */
export interface ChevronIconProps {
    /** Whether the message it controls is shown; the chevron then points up instead of down. */
    expanded: boolean;
}

/**
 * The chevron on the button of a banner that shows or hides its message. It is decorative: the button is named by its
 * label and says whether it is expanded.
 */
export const ChevronIcon = ({ expanded }: ChevronIconProps) => (
    <svg
        className={expanded ? 'marquee-chevron marquee-chevron--expanded' : 'marquee-chevron'}
        viewBox='0 0 24 24'
        width='1em'
        height='1em'
        aria-hidden='true'
        focusable='false'>
        <path fill='currentColor' d='M7.41 8.59 12 13.17l4.59-4.58L18 10l-6 6-6-6 1.41-1.41z' />
    </svg>
);

/**
 * Props for {@link ImportanceIcon}.
 */
export interface ImportanceIconProps {
    /** How the importance is presented. */
    presentation: ImportancePresentation;
}

/**
 * The icon of an importance, named for screen readers so the level is read out with the announcement.
 */
export const ImportanceIcon = ({ presentation }: ImportanceIconProps) => (
    <svg
        className={`marquee-icon marquee-icon--${presentation.modifier}`}
        viewBox='0 0 24 24'
        width='1em'
        height='1em'
        role='img'
        aria-label={presentation.label}
        focusable='false'>
        <path fill='currentColor' d={paths[presentation.modifier]} />
    </svg>
);
