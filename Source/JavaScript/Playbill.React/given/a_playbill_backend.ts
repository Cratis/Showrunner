// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ForMe, MarkReleaseNotesSeen } from '@cratis/showrunner.playbill';
import sinon from 'sinon';
import { PlaybillClient, Release } from '../Client/index.js';
import { summaryOf } from './releases.js';

/**
 * Stands in for the backend of Playbill: the endpoints serve the releases that are set, the status query returns what is
 * set, and the command records what it is asked to mark as seen. Restore it after each specification.
 */
export class a_playbill_backend {
    readonly marked: string[] = [];
    readonly requested: string[] = [];
    releases: Release[] = [];
    current?: string;
    unreadVersions: string[] = [];
    lastSeen = '';
    failing = false;
    readonly client: PlaybillClient;
    private readonly statusUse: sinon.SinonStub;
    private readonly markUse: sinon.SinonStub;

    constructor() {
        this.client = new PlaybillClient('/.cratis/playbill/v1', (input) => this.serve(String(input)));
        this.statusUse = sinon.stub(ForMe, 'use').callsFake((() => [
            { data: [{ current: this.current ?? '', lastSeen: this.lastSeen, unread: this.unreadVersions.length, unreadVersions: this.unreadVersions }] },
            () => { /* no sorting */ }
        ]) as never);
        const marked = this.marked;
        const command = {
            version: undefined as unknown as string,
            execute: () => {
                marked.push(command.version);
                return Promise.resolve({ isSuccess: true });
            }
        };
        this.markUse = sinon.stub(MarkReleaseNotesSeen, 'use').callsFake((() => [command, () => { /* no values */ }, () => { /* no clear */ }]) as never);
    }

    restore(): void {
        this.statusUse.restore();
        this.markUse.restore();
    }

    private serve(url: string): Promise<Response> {
        this.requested.push(url);
        if (this.failing) {
            return Promise.resolve(new Response('', { status: 500 }));
        }

        const path = url.replace('/.cratis/playbill/v1', '');
        if (path === '/releases') {
            return Promise.resolve(Response.json({ current: this.current, releases: this.releases.map(summaryOf) }));
        }

        const release = this.releases.find(candidate => path === `/releases/${encodeURIComponent(candidate.version)}`);
        return Promise.resolve(release ? Response.json(release) : new Response('', { status: 404 }));
    }
}
