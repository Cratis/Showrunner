// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * The icon of the release notes: a page with a ribbon. It is decorative, so it is hidden from assistive technology; the
 * control that holds it carries the accessible name.
 */
export const PlaybillIcon = () => (
    <svg className='playbill-icon' viewBox='0 0 24 24' width='1em' height='1em' fill='none' stroke='currentColor' strokeWidth='2' strokeLinecap='round' strokeLinejoin='round' aria-hidden='true' focusable='false'>
        <path d='M6 3h9l4 4v14H6z' />
        <path d='M14 3v5h5' />
        <path d='M9 13h7M9 17h7' />
    </svg>
);
