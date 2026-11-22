// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { PlaybillLabels } from './PlaybillLabels.js';

/**
 * The English texts Playbill shows unless a product replaces them.
 */
export const defaultPlaybillLabels: PlaybillLabels = {
    title: 'What\'s new',
    acknowledge: 'Got it',
    close: 'Close',
    releases: 'Releases',
    unread: 'New',
    current: 'Current',
    released: 'Released',
    loading: 'Loading release notes…',
    failed: 'The release notes could not be loaded.',
    empty: 'There are no release notes yet.',
    badge: 'Release notes',
    badgeUnread: 'Release notes, {count} new',
    earlier: '{count} earlier releases are on the release notes page.',
    added: 'Added',
    changed: 'Changed',
    fixed: 'Fixed',
    removed: 'Removed',
    security: 'Security',
    deprecated: 'Deprecated'
};
