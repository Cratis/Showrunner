// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import React from 'react';
import { cleanup, renderHook } from '@testing-library/react';
import { CueProvider, ResolvedCueConfiguration, useCueConfiguration } from '../CueProvider.js';
import type { CueSpeech } from '../../Speech/CueSpeech.js';

describe('when reading the configuration', () => {
    let configuration: ResolvedCueConfiguration;

    afterEach(cleanup);

    describe('and there is no provider', () => {
        beforeEach(() => { configuration = renderHook(() => useCueConfiguration()).result.current; });

        it('should have the tour play without a voice', () => configuration.useSpeech().isAvailable.should.be.false);
        it('should show English', () => configuration.labels.play.should.equal('Play'));
        it('should have a guard for the stage', () => (configuration.stageBoundary !== undefined).should.be.true);
    });

    describe('and the application gives its own', () => {
        const speech: CueSpeech = { isAvailable: true, isPreparing: false, isSpeaking: false, hasError: false, speak: () => Promise.resolve(), prefetch: () => undefined, stop: () => undefined };
        let unlocked: number;

        beforeEach(() => {
            unlocked = 0;
            configuration = renderHook(() => useCueConfiguration(), {
                wrapper: ({ children }) => <CueProvider speech={{ useSpeech: () => speech, unlock: () => { unlocked++; } }} labels={{ play: 'Spill av' }}>{children}</CueProvider>
            }).result.current;
        });

        it('should have the tour speak through it', () => configuration.useSpeech().isAvailable.should.be.true);
        it('should show its texts', () => configuration.labels.play.should.equal('Spill av'));
        it('should keep English for the rest', () => configuration.labels.pause.should.equal('Pause'));
        it('should unlock audio through it', () => {
            configuration.unlockAudio();
            unlocked.should.equal(1);
        });
    });
});
