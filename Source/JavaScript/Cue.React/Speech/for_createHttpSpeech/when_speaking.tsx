// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { act, cleanup, renderHook } from '@testing-library/react';
import { aSpeechService } from '../../given/aSpeechService.js';
import { createHttpSpeech } from '../createHttpSpeech.js';
import type { CueSpeech } from '../CueSpeech.js';

// The browser's audio element, as far as the voice-over needs it: it plays when told to, and the specification decides when
// the clip starts sounding, ends or fails.
class FakeAudio {
    static current: FakeAudio;
    onplaying: (() => void) | null = null;
    onended: (() => void) | null = null;
    onerror: (() => void) | null = null;
    src = '';
    paused = true;
    played: string[] = [];

    constructor() { FakeAudio.current = this; }

    play() {
        this.paused = false;
        this.played.push(this.src);
        return Promise.resolve();
    }

    pause() { this.paused = true; }
}

describe('when speaking', () => {
    let service: aSpeechService;
    let speech: CueSpeech;
    let unlock: (() => void) | undefined;
    let originalAudio: typeof Audio;
    let originalCreate: typeof URL.createObjectURL;
    let originalRevoke: typeof URL.revokeObjectURL;
    let revoked: string[];

    const prepare = (availableUrl?: string) => {
        const configuration = createHttpSpeech({ speakUrl: '/speak', availableUrl, fetch: service.fetch });
        unlock = configuration.unlock;
        const hook = renderHook(() => configuration.useSpeech());
        return hook;
    };

    beforeEach(() => {
        service = new aSpeechService();
        revoked = [];
        originalAudio = globalThis.Audio;
        originalCreate = URL.createObjectURL;
        originalRevoke = URL.revokeObjectURL;
        globalThis.Audio = FakeAudio as never;
        let counter = 0;
        URL.createObjectURL = () => `blob:clip-${++counter}`;
        URL.revokeObjectURL = (url: string) => { revoked.push(url); };
    });

    afterEach(() => {
        cleanup();
        globalThis.Audio = originalAudio;
        URL.createObjectURL = originalCreate;
        URL.revokeObjectURL = originalRevoke;
    });

    describe('and the service has no availability address', () => {
        beforeEach(() => { speech = prepare().result.current; });

        it('should take it to be available', () => speech.isAvailable.should.be.true);
    });

    describe('and the service says it is available', () => {
        beforeEach(async () => {
            const hook = prepare('/available');
            await act(async () => { await Promise.resolve(); });
            speech = hook.result.current;
        });

        it('should be available', () => speech.isAvailable.should.be.true);
    });

    describe('and the service says it is not available', () => {
        beforeEach(async () => {
            service.refuses = true;
            const hook = prepare('/available');
            await act(async () => { await Promise.resolve(); });
            speech = hook.result.current;
        });

        it('should not be available', () => speech.isAvailable.should.be.false);
    });

    describe('and a line is spoken', () => {
        let hook: ReturnType<typeof prepare>;
        let finished: Promise<void>;

        beforeEach(async () => {
            hook = prepare();
            await act(async () => {
                finished = hook.result.current.speak('Hello.');
                await Promise.resolve();
                await Promise.resolve();
            });
        });

        it('should ask the service for it', () => service.lines.should.deep.equal(['Hello.']));
        it('should play the clip', () => FakeAudio.current.played.should.deep.equal(['blob:clip-1']));
        it('should be preparing until the audio starts', () => hook.result.current.isPreparing.should.be.true);

        describe('and the audio starts', () => {
            beforeEach(() => act(() => { FakeAudio.current.onplaying!(); }));

            it('should be speaking', () => hook.result.current.isSpeaking.should.be.true);
            it('should no longer be preparing', () => hook.result.current.isPreparing.should.be.false);

            describe('and it ends', () => {
                beforeEach(async () => {
                    act(() => { FakeAudio.current.onended!(); });
                    await finished;
                });

                it('should stop speaking', () => hook.result.current.isSpeaking.should.be.false);
                it('should let the clip go', () => revoked.should.deep.equal(['blob:clip-1']));
            });

            describe('and it is stopped', () => {
                beforeEach(async () => {
                    act(() => hook.result.current.stop());
                    await finished;
                });

                it('should stop speaking', () => hook.result.current.isSpeaking.should.be.false);
                it('should silence the audio', () => FakeAudio.current.paused.should.be.true);
            });
        });

        describe('and the audio cannot be played', () => {
            beforeEach(() => act(() => { FakeAudio.current.onerror!(); }));

            it('should report the failure', () => hook.result.current.hasError.should.be.true);
            it('should not be preparing', () => hook.result.current.isPreparing.should.be.false);
        });
    });

    describe('and the service refuses the line', () => {
        beforeEach(async () => {
            service.refuses = true;
            const hook = prepare();
            await act(async () => { await hook.result.current.speak('Hello.'); });
            speech = hook.result.current;
        });

        it('should report the failure', () => speech.hasError.should.be.true);
        it('should not be speaking', () => speech.isSpeaking.should.be.false);
    });

    describe('and the next line is asked for ahead of time', () => {
        beforeEach(() => {
            speech = prepare().result.current;
            speech.prefetch('Next.');
        });

        it('should ask the service for it', () => service.lines.should.deep.equal(['Next.']));
    });

    describe('and audio is unlocked from a gesture', () => {
        beforeEach(() => {
            prepare();
            unlock!();
        });

        it('should play something silent', () => FakeAudio.current.played.should.have.length(1));
        it('should not play a line', () => service.lines.should.be.empty);
    });
});
