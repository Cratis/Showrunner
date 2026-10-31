// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { AcknowledgeAnnouncement, AnnouncementForMe, AnnouncementsForMe } from '@cratis/showrunner.marquee';
import sinon from 'sinon';

/**
 * Stands in for the backend of Marquee: the query returns what is set, and the command records what it is asked to
 * acknowledge. Restore it after each specification.
 */
export class a_marquee_backend {
    readonly acknowledged: string[] = [];
    announcements: AnnouncementForMe[] = [];

    private readonly use: sinon.SinonStub;
    private readonly acknowledgeUse: sinon.SinonStub;

    constructor() {
        this.use = sinon.stub(AnnouncementsForMe, 'use').callsFake((() => [{ data: this.announcements }, () => { /* no sorting */ }]) as never);

        const acknowledged = this.acknowledged;
        const command = {
            announcementId: undefined as unknown as { toString(): string },
            execute: () => {
                acknowledged.push(command.announcementId.toString());
                return Promise.resolve({ isSuccess: true });
            }
        };
        this.acknowledgeUse = sinon.stub(AcknowledgeAnnouncement, 'use').callsFake((() => [command, () => { /* no values */ }, () => { /* no clear */ }]) as never);
    }

    restore(): void {
        this.use.restore();
        this.acknowledgeUse.restore();
    }
}
