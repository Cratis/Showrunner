// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ByMe, StartTour } from '@cratis/showrunner.cue';
import { act, cleanup, renderHook } from '@testing-library/react';
import sinon from 'sinon';
import { useTourNudge } from '../useTourNudge.js';

describe('when deciding whether to point at a tour', () => {
    let started: { id: string }[];
    let isReady: boolean;
    let executed: string[];
    let query: sinon.SinonStub;
    let execute: sinon.SinonStub;

    beforeEach(() => {
        started = [];
        isReady = true;
        executed = [];
        query = sinon.stub(ByMe, 'use').callsFake((() => [{ data: started, isReady }, () => { /* no sorting */ }]) as never);
        execute = sinon.stub(StartTour.prototype, 'execute').callsFake(function (this: StartTour) {
            executed.push(this.tour);
            return Promise.resolve({ isSuccess: true } as never);
        });
    });

    afterEach(() => {
        cleanup();
        query.restore();
        execute.restore();
    });

    describe('and the person never started it', () => {
        it('should point at it', () => renderHook(() => useTourNudge('event-modeling')).result.current.isNudgeShown.should.be.true);
    });

    describe('and the person started it', () => {
        beforeEach(() => { started = [{ id: 'event-modeling' }]; });

        it('should not point at it', () => renderHook(() => useTourNudge('event-modeling')).result.current.isNudgeShown.should.be.false);
    });

    describe('and the person started another tour', () => {
        beforeEach(() => { started = [{ id: 'brainstorming' }]; });

        it('should still point at this one', () => renderHook(() => useTourNudge('event-modeling')).result.current.isNudgeShown.should.be.true);
    });

    describe('and what the person started has not arrived yet', () => {
        beforeEach(() => { isReady = false; });

        it('should not point at it', () => renderHook(() => useTourNudge('event-modeling')).result.current.isNudgeShown.should.be.false);
    });

    describe('and the surface says not to', () => {
        it('should not point at it', () => renderHook(() => useTourNudge('event-modeling', true)).result.current.isNudgeShown.should.be.false);
    });

    describe('and the tour is started', () => {
        let hook: ReturnType<typeof renderHook<ReturnType<typeof useTourNudge>, unknown>>;

        beforeEach(async () => {
            hook = renderHook(() => useTourNudge('event-modeling'));
            await act(async () => { hook.result.current.recordStart(); await Promise.resolve(); });
        });

        it('should record it', () => executed.should.deep.equal(['event-modeling']));
        it('should stop pointing at once', () => hook.result.current.isNudgeShown.should.be.false);
    });
});
