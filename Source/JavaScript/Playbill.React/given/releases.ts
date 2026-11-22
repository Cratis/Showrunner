// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Release, ReleaseSummary } from '../Client/index.js';

/**
 * Makes a release with a note, for specifications.
 * @param overrides What to set; the rest is a release of 1.0.0 with an Added section.
 * @returns The release.
 */
export const aRelease = (overrides: Partial<Release> = {}): Release => ({
    version: '1.0.0',
    releasedAt: '2026-03-02T10:00:00Z',
    pull: '12',
    summary: 'The first release.',
    lead: '',
    sections: [{ name: 'Added', markdown: '- The first release.' }],
    assets: [],
    markdown: '## Added\n\n- The first release.',
    ...overrides
});

/**
 * Describes a release as the list does.
 * @param release The release.
 * @returns The list entry.
 */
export const summaryOf = (release: Release): ReleaseSummary => ({
    version: release.version,
    releasedAt: release.releasedAt,
    pull: release.pull,
    summary: release.summary,
    sections: release.sections.map(section => section.name),
    assets: release.assets
});
