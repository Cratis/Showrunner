// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { PlaybillClient } from '../PlaybillClient.js';

describe('when fetching', () => {
    let requests: { url: string; init?: RequestInit }[];
    let response: Response;
    let client: PlaybillClient;

    beforeEach(() => {
        requests = [];
        response = Response.json({ releases: [{ version: '1.0.0', summary: 'x', sections: [], assets: [] }], current: '1.0.0' });
        client = new PlaybillClient(undefined, (input, init) => {
            requests.push({ url: String(input), init });
            return Promise.resolve(response);
        });
    });

    describe('and the releases are listed', () => {
        let list: Awaited<ReturnType<PlaybillClient['list']>>;
        beforeEach(async () => { list = await client.list(); });

        it('should ask the versioned endpoint', () => requests[0].url.should.equal('/.cratis/playbill/v1/releases'));
        it('should ask for json', () => (requests[0].init!.headers as Record<string, string>).Accept.should.equal('application/json'));
        it('should send the cookies of the page', () => requests[0].init!.credentials!.should.equal('same-origin'));
        it('should give the releases', () => list.releases.map(_ => _.version).should.deep.equal(['1.0.0']));
    });

    describe('and the releases since a version are listed', () => {
        beforeEach(() => client.list('1.2.0 beta'));

        it('should pass the version, encoded', () => requests[0].url.should.equal('/.cratis/playbill/v1/releases?since=1.2.0%20beta'));
    });

    describe('and one release is read', () => {
        beforeEach(() => client.get('2.0.0-rc.1'));

        it('should ask for that release', () => requests[0].url.should.equal('/.cratis/playbill/v1/releases/2.0.0-rc.1'));
    });

    describe('and the current release is read', () => {
        beforeEach(() => client.current());

        it('should ask the current endpoint', () => requests[0].url.should.equal('/.cratis/playbill/v1/current'));
    });

    describe('and the base path is changed', () => {
        beforeEach(() => new PlaybillClient('/api/notes', (input) => { requests.push({ url: String(input) }); return Promise.resolve(response); }).list());

        it('should use it', () => requests[0].url.should.equal('/api/notes/releases'));
        it('should serve the files of a release from under it', () => new PlaybillClient('/api/notes').assetBase('1.0.0').should.equal('/api/notes/releases/1.0.0'));
    });

    describe('and the endpoint refuses', () => {
        let failure: Error | undefined;
        beforeEach(async () => {
            response = new Response('', { status: 401, statusText: 'Unauthorized' });
            failure = await client.list().then(() => undefined, (error: Error) => error);
        });

        it('should fail', () => failure!.message.should.contain('401'));
    });
});
