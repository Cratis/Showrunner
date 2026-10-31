// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '@cratis/fundamentals';
import { anAnnouncement } from '../../given/an_announcement.js';
import { isActiveAt, selectAnnouncements } from '../announcementSelection.js';

describe('when applying the window', () => {
    const from = new Date('2026-11-01T10:00:00Z');
    const until = new Date('2026-11-01T18:00:00Z');
    const announcement = anAnnouncement({ activeFrom: from, activeUntil: until });

    it('should not show before it starts', () => isActiveAt(announcement, new Date('2026-11-01T09:59:59Z')).should.be.false);
    it('should show the moment it starts', () => isActiveAt(announcement, from).should.be.true);
    it('should show inside the window', () => isActiveAt(announcement, new Date('2026-11-01T14:00:00Z')).should.be.true);
    it('should be gone the moment it ends', () => isActiveAt(announcement, until).should.be.false);

    describe('and it is selected before it starts', () => {
        const selection = selectAnnouncements([announcement], new Date('2026-11-01T09:00:00Z'), new Set());

        it('should show no banner', () => selection.banners.should.be.empty);
        it('should show no dialog', () => selection.dialogs.should.be.empty);
    });

    describe('and the dialog is not yet due', () => {
        const dialog = anAnnouncement({ display: 1, activeFrom: from, activeUntil: until });
        const selection = selectAnnouncements([dialog], new Date('2026-11-01T09:00:00Z'), new Set());

        it('should show no dialog', () => selection.dialogs.should.be.empty);
    });

    describe('and a banner has no end', () => {
        const forever = anAnnouncement({ activeFrom: new Date('0001-01-01T00:00:00Z'), activeUntil: new Date('9999-12-31T23:59:59Z') });

        it('should show it now', () => isActiveAt(forever, new Date()).should.be.true);
    });

    it('should not mix up ids', () => Guid.create().toString().should.not.equal(announcement.id.toString()));
});
