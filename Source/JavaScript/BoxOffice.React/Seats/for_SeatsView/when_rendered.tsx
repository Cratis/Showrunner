// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { cleanup, fireEvent, render } from '@testing-library/react';
import { expect } from 'chai';
import sinon from 'sinon';
import { BoxOfficeHarness } from '../../given/BoxOfficeHarness.js';
import { ada, bo, cy, holding, rosterOf } from '../../given/boxOfficeData.js';
import { seatRows, seatSummary } from '../seatModels.js';
import { SeatsView, SeatsViewProps } from '../SeatsView.js';

const renderSeats = (capacity: number, isReadOnly = false, overrides: Partial<SeatsViewProps> = {}) => {
    const rows = seatRows(rosterOf(holding(ada, 9), holding(bo, 4), holding(cy, 1)), capacity);
    const onAssign = sinon.stub();
    const onRelease = sinon.stub();
    const result = render(
        <BoxOfficeHarness>
            <SeatsView rows={rows} summary={seatSummary(rows, capacity)} isReadOnly={isReadOnly} onAssign={onAssign} onRelease={onRelease} {...overrides} />
        </BoxOfficeHarness>
    );
    return { result, onAssign, onRelease };
};

describe('when rendered', () => {
    afterEach(() => cleanup());

    it('should say how many seats are in use', () => (renderSeats(10).result.queryByText('3 of 10 seats in use') !== null).should.be.true);
    it('should list who holds a seat', () => (renderSeats(10).result.queryByText('Ada Lovelace') !== null).should.be.true);
    it('should mark a seat the tenant has no room for', () => (renderSeats(2).result.queryByText('Not covered') !== null).should.be.true);
    it('should warn about the people over', () => (renderSeats(2).result.queryByText('Your organization has 1 more people with seats than it has bought.') !== null).should.be.true);
    it('should say the tenant has no seats', () => (renderSeats(0).result.queryByText('Your organization has no seats.') !== null).should.be.true);
    it('should not allow assigning when the tenant has no seats', () => (renderSeats(0).result.getByRole('button', { name: 'Assign a seat' }) as HTMLButtonElement).disabled.should.be.true);
    it('should not allow assigning when the tenant is read only', () => (renderSeats(10, true).result.getByRole('button', { name: 'Assign a seat' }) as HTMLButtonElement).disabled.should.be.true);
    it('should say why seats cannot be changed', () => (renderSeats(10, true).result.queryByText('Seats cannot be changed while your organization is read-only.') !== null).should.be.true);

    it('should ask to assign a seat', () => {
        const { result, onAssign } = renderSeats(10);
        fireEvent.click(result.getByRole('button', { name: 'Assign a seat' }));
        expect(onAssign.calledOnce).to.equal(true);
    });

    it('should ask to release the seat of the person clicked', () => {
        const { result, onRelease } = renderSeats(10);
        fireEvent.click(result.getByRole('button', { name: 'Release the seat of Bo Nilsen' }));
        onRelease.firstCall.args[0].holder.name.should.equal('Bo Nilsen');
    });

    it('should say nobody holds a seat when there are none', () => {
        const { result } = renderSeats(10, false, { rows: [], summary: { held: 0, capacity: 10, over: 0 } });
        expect(result.queryByText('Nobody holds a seat yet.')).to.not.equal(null);
    });
});
