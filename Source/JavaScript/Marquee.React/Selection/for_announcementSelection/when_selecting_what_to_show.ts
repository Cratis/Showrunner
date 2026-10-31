// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Display, Importance } from '@cratis/showrunner.marquee';
import { anAnnouncement } from '../../given/an_announcement.js';
import { selectAnnouncements } from '../announcementSelection.js';

describe('when selecting what to show', () => {
    const now = new Date('2026-11-01T12:00:00Z');
    const information = anAnnouncement({ title: 'information', display: Display.dialogOnce, importance: Importance.information });
    const critical = anAnnouncement({ title: 'critical', display: Display.dialogEverySession, importance: Importance.critical });
    const warning = anAnnouncement({ title: 'warning', display: Display.dialogOnce, importance: Importance.warning, activeFrom: new Date('2026-03-01T00:00:00Z') });
    const olderWarning = anAnnouncement({ title: 'older warning', display: Display.dialogOnce, importance: Importance.warning, activeFrom: new Date('2026-02-01T00:00:00Z') });
    const bannerInformation = anAnnouncement({ title: 'banner information', display: Display.banner, importance: Importance.information });
    const bannerCritical = anAnnouncement({ title: 'banner critical', display: Display.banner, importance: Importance.critical });

    describe('and nothing was closed', () => {
        const selection = selectAnnouncements([information, critical, warning, olderWarning, bannerInformation, bannerCritical], now, new Set());

        it('should show the dialogs the most important first, and the oldest window first among equals',
            () => selection.dialogs.map(_ => _.title).should.deep.equal(['critical', 'older warning', 'warning', 'information']));
        it('should show the banners the most important first', () => selection.banners.map(_ => _.title).should.deep.equal(['banner critical', 'banner information']));
    });

    describe('and a dialog was closed', () => {
        const selection = selectAnnouncements([information, critical], now, new Set([critical.id.toString()]));

        it('should not offer it again', () => selection.dialogs.map(_ => _.title).should.deep.equal(['information']));
    });

    describe('and the id of a banner is among the closed', () => {
        const selection = selectAnnouncements([bannerInformation], now, new Set([bannerInformation.id.toString()]));

        it('should still show the banner, since a banner can not be closed', () => selection.banners.should.have.length(1));
    });
});
