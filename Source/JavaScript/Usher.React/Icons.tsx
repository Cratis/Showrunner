// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * The bug icon of the report button. Decorative: the button carries the accessible name.
 */
export const BugIcon = () => (
    <svg className='usher-icon' viewBox='0 0 24 24' width='1em' height='1em' aria-hidden='true' focusable='false'>
        <path fill='currentColor' d='M20 8h-2.81a5.985 5.985 0 0 0-1.82-1.96L17 4.41 15.59 3l-2.17 2.17a6.002 6.002 0 0 0-2.83 0L8.41 3 7 4.41l1.62 1.63C7.88 6.55 7.26 7.22 6.81 8H4v2h2.09c-.05.33-.09.66-.09 1v1H4v2h2v1c0 .34.04.67.09 1H4v2h2.81c1.04 1.79 2.97 3 5.19 3s4.15-1.21 5.19-3H20v-2h-2.09c.05-.33.09-.66.09-1v-1h2v-2h-2v-1c0-.34-.04-.67-.09-1H20V8zm-6 8h-4v-2h4v2zm0-4h-4v-2h4v2z' />
    </svg>
);

/**
 * The inbox icon of the my issues button. Decorative: the button carries the accessible name.
 */
export const InboxIcon = () => (
    <svg className='usher-icon' viewBox='0 0 24 24' width='1em' height='1em' aria-hidden='true' focusable='false'>
        <path fill='currentColor' d='M19 3H5a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2V5a2 2 0 0 0-2-2zm0 12h-4a3 3 0 0 1-6 0H5V5h14v10z' />
    </svg>
);

/**
 * The back arrow of the issue view. Decorative: the button carries the accessible name.
 */
export const BackIcon = () => (
    <svg className='usher-icon' viewBox='0 0 24 24' width='1em' height='1em' aria-hidden='true' focusable='false'>
        <path fill='currentColor' d='M20 11H7.83l5.59-5.59L12 4l-8 8 8 8 1.41-1.41L7.83 13H20v-2z' />
    </svg>
);
