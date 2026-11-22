// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { PlaybillLabels } from '../Labels/index.js';

/**
 * How a section of a release note is drawn.
 */
export interface SectionPresentation {
    /** The heading to show; the text the product gave for a known section, and the heading as written for any other. */
    title: string;
    /** The modifier of the `playbill-section` class, such as `added`; `other` for a heading that is not a known one. */
    modifier: string;
}

const known: Record<string, keyof PlaybillLabels> = {
    added: 'added',
    changed: 'changed',
    fixed: 'fixed',
    removed: 'removed',
    security: 'security',
    deprecated: 'deprecated'
};

/**
 * Works out how a section of a release note is drawn. The sections of a release note are the six the release notes of
 * every Cratis repository use; a heading written any other way is shown as it was written.
 * @param name The heading of the section.
 * @param labels The texts to show.
 * @returns The title and the style of the section.
 */
export const sectionPresentation = (name: string, labels: PlaybillLabels): SectionPresentation => {
    const key = known[name.trim().toLowerCase()];
    return key ? { title: labels[key], modifier: key } : { title: name, modifier: 'other' };
};
