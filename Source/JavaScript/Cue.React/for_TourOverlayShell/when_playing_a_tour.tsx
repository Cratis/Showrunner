// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import React from 'react';
import sinon from 'sinon';
import { act, cleanup, fireEvent, render, RenderResult } from '@testing-library/react';
import { CueProvider } from '../Configuration/CueProvider.js';
import { sampleTour } from '../given/sampleTour.js';
import type { CueSpeech } from '../Speech/CueSpeech.js';
import { TourOverlayShell } from '../TourOverlayShell.js';

describe('when playing a tour', () => {
    let spoken: string[];
    let prefetched: string[];
    let unlocked: number;
    let closed: number;
    let speech: CueSpeech;
    let result: RenderResult;
    let clock: sinon.SinonFakeTimers;
    let tree: () => React.ReactElement;

    const tour = () => <TourOverlayShell sections={sampleTour} onClose={() => { closed++; }}>{() => <div data-testid='stage' />}</TourOverlayShell>;

    // The voice is read afresh on every render, as the application's own hook would be, so a specification can change what it
    // says it is doing and render again.
    const play = (overrides: Partial<CueSpeech> = {}, withVoice = true) => {
        speech = {
            isAvailable: true,
            isPreparing: false,
            isSpeaking: false,
            hasError: false,
            speak: text => { spoken.push(text); return Promise.resolve(); },
            prefetch: text => { prefetched.push(text); },
            stop: () => undefined,
            ...overrides
        };
        tree = () => withVoice
            ? <CueProvider speech={{ useSpeech: () => speech, unlock: () => { unlocked++; } }}>{tour()}</CueProvider>
            : tour();
        result = render(tree());
    };

    const has = (text: string) => result.queryAllByText(text).length > 0;
    const hasTitle = (title: string) => result.queryAllByTitle(title).length > 0;
    const firstLine = 'This is the order desk. Everything a customer asks for lands here first.';

    beforeEach(() => {
        clock = sinon.useFakeTimers({ toFake: ['setInterval', 'clearInterval', 'performance'] });
        spoken = [];
        prefetched = [];
        unlocked = 0;
        closed = 0;
    });

    afterEach(() => {
        cleanup();
        clock.restore();
    });

    describe('and the application has a voice', () => {
        beforeEach(() => play());

        it('should draw the stage', () => (result.queryAllByTestId('stage').length > 0).should.be.true);
        it('should speak the first line', () => spoken.should.deep.equal([firstLine]));
        it('should show the subtitles', () => has(firstLine).should.be.true);
        it('should list the chapters', () => has('Picking').should.be.true);
        it('should ask for nothing else while the first line is not audible yet', () => prefetched.should.be.empty);

        describe('and the viewer presses pause', () => {
            beforeEach(() => fireEvent.click(result.getByTitle('Pause')));

            it('should unlock audio from the gesture', () => unlocked.should.equal(1));
            it('should offer to play again', () => hasTitle('Play').should.be.true);
        });

        describe('and the viewer closes the tour', () => {
            beforeEach(() => fireEvent.click(result.getByTitle('Close the tour')));

            it('should hand the surface back', () => closed.should.equal(1));
        });

        describe('and the viewer presses escape', () => {
            beforeEach(() => fireEvent.keyDown(window, { key: 'Escape' }));

            it('should hand the surface back', () => closed.should.equal(1));
        });

        describe('and time passes', () => {
            beforeEach(() => act(() => { clock.tick(1000); }));

            it('should keep playing', () => hasTitle('Pause').should.be.true);
        });

        describe('and the first line starts to be heard', () => {
            beforeEach(() => {
                speech = { ...speech, isPreparing: true };
                act(() => { result.rerender(tree()); });
                speech = { ...speech, isPreparing: false, isSpeaking: true };
                act(() => { result.rerender(tree()); });
            });

            it('should ask ahead for the line to come', () => prefetched.should.include('Each order is a card. The newest sits on top.'));
        });
    });

    describe('and the voice fails', () => {
        beforeEach(() => play({ hasError: true }));

        it('should say so', () => hasTitle('Voice-over failed — subtitles carry on. Press to retry').should.be.true);
    });

    describe('and the application has no voice', () => {
        beforeEach(() => play({}, false));

        it('should say there is none', () => hasTitle('Voice-over is not available').should.be.true);
        it('should still show the subtitles', () => has(firstLine).should.be.true);
        it('should not offer written chapters', () => hasTitle('Read the tour at your own pace').should.be.false);
    });
});
