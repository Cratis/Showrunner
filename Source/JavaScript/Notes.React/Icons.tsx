// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * The speech bubble icon of the feedback button. Decorative: the button carries the accessible name.
 */
export const FeedbackIcon = () => (
    <svg className='notes-icon' viewBox='0 0 24 24' width='1em' height='1em' aria-hidden='true' focusable='false'>
        <path fill='currentColor' d='M20 2H4a2 2 0 0 0-2 2v18l4-4h14a2 2 0 0 0 2-2V4a2 2 0 0 0-2-2zm0 14H5.17L4 17.17V4h16v12z' />
    </svg>
);

/**
 * The chevron of the drop-down toggle. Decorative: the toggle carries the accessible name.
 */
export const ChevronIcon = () => (
    <svg className='notes-icon' viewBox='0 0 24 24' width='1em' height='1em' aria-hidden='true' focusable='false'>
        <path fill='currentColor' d='M7.41 8.59 12 13.17l4.59-4.58L18 10l-6 6-6-6 1.41-1.41z' />
    </svg>
);

/**
 * The close icon of a chosen slice's remove button. Decorative: the button carries the accessible name.
 */
export const CloseIcon = () => (
    <svg className='notes-icon' viewBox='0 0 24 24' width='1em' height='1em' aria-hidden='true' focusable='false'>
        <path fill='currentColor' d='M19 6.41 17.59 5 12 10.59 6.41 5 5 6.41 10.59 12 5 17.59 6.41 19 12 13.41 17.59 19 19 17.59 13.41 12 19 6.41z' />
    </svg>
);
