// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Display } from '@cratis/showrunner.marquee';
import { anAnnouncement } from '../../given/an_announcement.js';
import { isActiveAt, selectAnnouncements } from '../announcementSelection.js';

describe('when the window is missing', () => {
    const now = new Date('2026-11-01T14:00:00Z');
    const permanent = anAnnouncement({ activeFrom: new Date('0001-01-01T00:00:00Z'), activeUntil: new Date('9999-12-31T23:59:59Z') });
    const withoutStart = anAnnouncement({ activeUntil: new Date('2026-11-01T18:00:00Z') });
    const withoutEnd = anAnnouncement({ activeFrom: new Date('2026-11-01T10:00:00Z') });
    const noDates = anAnnouncement();

    (withoutStart as { activeFrom?: Date }).activeFrom = undefined;
    (withoutEnd as { activeUntil?: Date }).activeUntil = undefined;
    (noDates as { activeFrom?: Date }).activeFrom = undefined;
    (noDates as { activeUntil?: Date }).activeUntil = undefined;

    describe('and it is permanent', () => {
        it('should show it now', () => isActiveAt(permanent, now).should.be.true);
        it('should show it in a hundred years', () => isActiveAt(permanent, new Date('2126-11-01T14:00:00Z')).should.be.true);
    });

    describe('and there is no start', () => {
        it('should show it now', () => isActiveAt(withoutStart, now).should.be.true);
        it('should be gone when it ends', () => isActiveAt(withoutStart, new Date('2026-11-01T18:00:00Z')).should.be.false);
    });

    describe('and there is no end', () => {
        it('should not show it before it starts', () => isActiveAt(withoutEnd, new Date('2026-11-01T09:00:00Z')).should.be.false);
        it('should show it once it has started', () => isActiveAt(withoutEnd, now).should.be.true);
        it('should keep showing it', () => isActiveAt(withoutEnd, new Date('2099-01-01T00:00:00Z')).should.be.true);
    });

    describe('and there are neither', () => {
        it('should show it', () => isActiveAt(noDates, now).should.be.true);

        describe('and it is selected as a banner', () => {
            const selection = selectAnnouncements([noDates], now, new Set());

            it('should show the banner', () => selection.banners.should.have.length(1));
        });

        describe('and it is a dialog that was closed', () => {
            const dialog = anAnnouncement({ display: Display.dialogOnce });
            (dialog as { activeFrom?: Date }).activeFrom = undefined;
            (dialog as { activeUntil?: Date }).activeUntil = undefined;

            it('should show the dialog until it is closed', () => selectAnnouncements([dialog], now, new Set()).dialogs.should.have.length(1));
            it('should not offer it again once it is closed', () => selectAnnouncements([dialog], now, new Set([dialog.id.toString()])).dialogs.should.be.empty);
        });
    });
});
