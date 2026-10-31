// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '@cratis/fundamentals';
import { AuthorAnnouncement, Audience, AudienceKind, Display, Importance } from '@cratis/showrunner.marquee.authoring';

export const authorMaintenanceNotice = async () => {
    const audience = new Audience();
    audience.kind = AudienceKind.everyone;
    audience.tenantIds = [];
    audience.module = '';
    audience.plan = '';

    const author = new AuthorAnnouncement();
    author.announcementId = Guid.create();
    author.product = 'ledger';
    author.audience = audience;
    author.title = 'Planned maintenance';
    author.body = 'Ledger is read-only on **Saturday** from 22:00 to 23:00 UTC.';
    author.display = Display.banner;
    author.importance = Importance.warning;
    author.activeFrom = new Date('2026-11-06T00:00:00Z');
    author.activeUntil = new Date('2026-11-08T00:00:00Z');

    const result = await author.execute();
    return result.isSuccess;
};
