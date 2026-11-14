// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ByMe, StartTour } from '@cratis/showrunner.cue';
import { act, cleanup, renderHook } from '@testing-library/react';
import sinon from 'sinon';
import { useCueTours } from '../useCueTours.js';

describe('when recording a start', () => {
    let startedOnTheServer: { id: string }[];
    let isReady: boolean;
    let executed: string[];
    let query: sinon.SinonStub;
    let execute: sinon.SinonStub;

    const render = () => renderHook(() => useCueTours());

    beforeEach(() => {
        startedOnTheServer = [{ id: 'event-modeling' }];
        isReady = true;
        executed = [];
        query = sinon.stub(ByMe, 'use').callsFake((() => [{ data: startedOnTheServer, isReady }, () => { /* no query */ }, () => { /* no sorting */ }]) as never);
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

    describe('and what the person started has arrived', () => {
        it('should be loaded', () => render().result.current.isLoaded.should.be.true);
        it('should know a tour they started', () => render().result.current.hasStarted('event-modeling').should.be.true);
        it('should not know a tour they did not', () => render().result.current.hasStarted('brainstorming').should.be.false);
    });

    describe('and it has not arrived yet', () => {
        beforeEach(() => { isReady = false; startedOnTheServer = []; });

        it('should not be loaded', () => render().result.current.isLoaded.should.be.false);
    });

    describe('and the person starts a tour they never started', () => {
        let hook: ReturnType<typeof render>;

        beforeEach(async () => {
            hook = render();
            await act(async () => { await hook.result.current.recordStart('brainstorming'); });
        });

        it('should record it', () => executed.should.deep.equal(['brainstorming']));
        it('should count at once, before the server has caught up', () => hook.result.current.hasStarted('brainstorming').should.be.true);

        describe('and starts it again', () => {
            beforeEach(() => act(async () => { await hook.result.current.recordStart('brainstorming'); }));

            it('should not record it twice', () => executed.should.have.length(1));
        });
    });

    describe('and the person starts a tour they already started', () => {
        beforeEach(async () => {
            const hook = render();
            await act(async () => { await hook.result.current.recordStart('event-modeling'); });
        });

        it('should record nothing', () => executed.should.be.empty);
    });
});
