// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * The texts Marquee shows. Every one is a prop so a product can localize it.
 */
export interface MarqueeLabels {
    /** The accessible name of the region the banners are announced in. */
    banners: string;

    /** The button that closes a dialog when no other follows. */
    acknowledge: string;

    /** The button that moves on to the next dialog when more follow. */
    next: string;

    /** The accessible name of the close button in the header of a dialog. */
    close: string;

    /** The accessible name of the icon of an announcement of information importance. */
    information: string;

    /** The accessible name of the icon of an announcement of warning importance. */
    warning: string;

    /** The accessible name of the icon of an announcement of critical importance. */
    critical: string;
}
