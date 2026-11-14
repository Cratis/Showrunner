// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '@cratis/fundamentals';
import { AuthorAnnouncement, Audience, AudienceKind, Display, Importance } from '@cratis/showrunner.marquee.authoring';

export const authorPermanentNotice = async () => {
    const audience = new Audience();
    audience.kind = AudienceKind.everyone;
    audience.tenantIds = [];
    audience.module = '';
    audience.plan = '';

    const author = new AuthorAnnouncement();
    author.announcementId = Guid.create();
    author.product = 'ledger';
    author.audience = audience;
    author.title = 'Ledger moves to a new address. [Read more here.](https://status.example.com/new-address)';
    author.display = Display.banner;
    author.importance = Importance.information;

    // No body, no activeFrom and no activeUntil: the notice is only a title, and stays until it is withdrawn.
    const result = await author.execute();
    return result.isSuccess;
};
