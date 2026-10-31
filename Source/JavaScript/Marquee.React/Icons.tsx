// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ImportancePresentation } from './Selection/index.js';

const paths: Record<ImportancePresentation['modifier'], string> = {
    information: 'M12 2a10 10 0 1 0 0 20 10 10 0 0 0 0-20zm1 15h-2v-6h2v6zm0-8h-2V7h2v2z',
    warning: 'M1 21h22L12 2 1 21zm12-3h-2v-2h2v2zm0-4h-2v-4h2v4z',
    critical: 'M12 2a10 10 0 1 0 0 20 10 10 0 0 0 0-20zm1 15h-2v-2h2v2zm0-4h-2V7h2v6z'
};

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
