// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * The texts Playbill shows. Every one is a prop so a product can localize it. A text with `{count}` has the number put in.
 */
export interface PlaybillLabels {
    /** The heading of the page and of the dialog. */
    title: string;
    /** The button that closes the dialog. */
    acknowledge: string;
    /** The accessible name of the close button in the header of the dialog. */
    close: string;
    /** The accessible name of the list of releases. */
    releases: string;
    /** The tag on a release the person has not seen the notes of. */
    unread: string;
    /** The tag on the release the application is running. */
    current: string;
    /** The word before the date a release was made. */
    released: string;
    /** Shown while the release notes are being read. */
    loading: string;
    /** Shown when the release notes could not be read. */
    failed: string;
    /** Shown when the application has no release notes. */
    empty: string;
    /** The accessible name of the badge when nothing is unread. */
    badge: string;
    /** The accessible name of the badge when releases are unread; has `{count}`. */
    badgeUnread: string;
    /** Shown in the dialog when it holds fewer releases than are unread; has `{count}`. */
    earlier: string;
    /** The heading of the `Added` section. */
    added: string;
    /** The heading of the `Changed` section. */
    changed: string;
    /** The heading of the `Fixed` section. */
    fixed: string;
    /** The heading of the `Removed` section. */
    removed: string;
    /** The heading of the `Security` section. */
    security: string;
    /** The heading of the `Deprecated` section. */
    deprecated: string;
}
