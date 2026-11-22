// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * A release in the list of releases, as `GET /.cratis/playbill/v1/releases` gives it.
 */
export interface ReleaseSummary {
    /** The version of the release, such as `2.3.0`. */
    version: string;
    /** When the release was made, as an ISO date; absent when the capture did not record it. */
    releasedAt?: string | null;
    /** The pull request the release was made from; absent when the capture did not record it. */
    pull?: string | null;
    /** One line that says what the release is about. */
    summary: string;
    /** The headings of the note, such as `Added` and `Fixed`. */
    sections: string[];
    /** The files the note refers to, such as images. */
    assets: string[];
}

/**
 * One heading of a release note and what is written under it.
 */
export interface ReleaseSection {
    /** The heading, such as `Added`. */
    name: string;
    /** The markdown under the heading. */
    markdown: string;
}

/**
 * One release with its note, as `GET /.cratis/playbill/v1/releases/{version}` gives it when JSON is asked for.
 */
export interface Release extends Omit<ReleaseSummary, 'sections'> {
    /** What is written before the first section, as markdown; empty when the note starts with a section. */
    lead: string;
    /** The sections of the note, in the order they are written. */
    sections: ReleaseSection[];
    /** The note as it was captured. */
    markdown: string;
}

/**
 * The list of releases.
 */
export interface ReleaseList {
    /** The version the application is running; absent when it is not known. */
    current?: string | null;
    /** The releases, the newest first. */
    releases: ReleaseSummary[];
}

/**
 * The version the application is running, as `GET /.cratis/playbill/v1/current` gives it.
 */
export interface CurrentRelease {
    /** The version; absent when it is not known. */
    version?: string | null;
    /** The release of that version; absent when it has no note. */
    release?: ReleaseSummary | null;
    /** The newest release that has a note; absent when none has. */
    newest?: string | null;
}
