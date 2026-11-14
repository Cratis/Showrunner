// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { aSpeechService } from '../../given/aSpeechService.js';
import { createSpeechClips } from '../speechClips.js';

describe('when asking for a line', () => {
    let service: aSpeechService;

    beforeEach(() => { service = new aSpeechService(); });

    describe('and it is asked for twice', () => {
        beforeEach(async () => {
            const clips = createSpeechClips({ speakUrl: '/speak', fetch: service.fetch });
            await clips.obtain('Hello.');
            await clips.obtain('Hello.');
        });

        it('should ask the service once', () => service.lines.should.deep.equal(['Hello.']));
    });

    describe('and it is asked for ahead of time and then spoken', () => {
        beforeEach(async () => {
            const clips = createSpeechClips({ speakUrl: '/speak', fetch: service.fetch });
            clips.prefetch('Hello.');
            await clips.obtain('Hello.');
        });

        it('should ask the service once', () => service.lines.should.deep.equal(['Hello.']));
    });

    describe('and the service refuses', () => {
        let failure: Error | undefined;
        let clips: ReturnType<typeof createSpeechClips>;

        beforeEach(async () => {
            service.refuses = true;
            clips = createSpeechClips({ speakUrl: '/speak', fetch: service.fetch });
            try {
                await clips.obtain('Hello.');
            } catch (error) {
                failure = error as Error;
            }
        });

        it('should fail', () => failure!.message.should.contain('503'));

        it('should ask again the next time', async () => {
            service.refuses = false;
            await clips.obtain('Hello.');
            service.lines.should.have.length(2);
        });
    });

    describe('and the application builds the request', () => {
        beforeEach(async () => {
            const clips = createSpeechClips({
                speakUrl: '/speak',
                fetch: service.fetch,
                headers: { 'X-Key': 'secret' },
                credentials: 'omit',
                body: text => ({ text, voice: 'aoede' })
            });
            await clips.obtain('Hello.');
        });

        it('should send what it built', () => service.requests[0].init.body!.should.equal('{"text":"Hello.","voice":"aoede"}'));
        it('should send its headers', () => (service.requests[0].init.headers as Record<string, string>)['X-Key'].should.equal('secret'));
        it('should send its credentials setting', () => service.requests[0].init.credentials!.should.equal('omit'));
        it('should send the line as JSON', () => (service.requests[0].init.headers as Record<string, string>)['Content-Type'].should.equal('application/json'));
    });

    describe('and more lines are asked for than the cache holds', () => {
        beforeEach(async () => {
            const clips = createSpeechClips({ speakUrl: '/speak', fetch: service.fetch, cacheSize: 2 });
            await clips.obtain('One.');
            await clips.obtain('Two.');
            await clips.obtain('Three.');
            await clips.obtain('One.');
        });

        it('should ask for the oldest again', () => service.lines.should.deep.equal(['One.', 'Two.', 'Three.', 'One.']));
    });
});
