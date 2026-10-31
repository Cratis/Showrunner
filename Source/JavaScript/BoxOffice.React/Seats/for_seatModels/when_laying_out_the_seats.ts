// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ada, bo, cy, holding, rosterOf } from '../../given/boxOfficeData.js';
import { seatRows, seatSummary } from '../seatModels.js';

describe('when laying out the seats', () => {
    const roster = rosterOf(holding(cy, 1), holding(ada, 9), holding(bo, 4));
    const rows = seatRows(roster, 2);
    const summary = seatSummary(rows, 2);

    it('should list the oldest seat first', () => rows.map(_ => _.holding.holder.name).should.deep.equal(['Ada Lovelace', 'Bo Nilsen', 'Cy Hansen']));
    it('should cover the first seats given', () => rows.slice(0, 2).every(_ => _.coverage === 'licensed').should.be.true);
    it('should not cover the seats beyond the capacity', () => rows[2].coverage.should.equal('overCapacity'));
    it('should count the seats held', () => summary.held.should.equal(3));
    it('should tell the capacity', () => summary.capacity.should.equal(2));
    it('should count the people over', () => summary.over.should.equal(1));
    it('should cover nobody when the tenant has no seats', () => seatRows(roster, 0).every(_ => _.coverage === 'overCapacity').should.be.true);
    it('should list nothing without a roster', () => seatRows(undefined, 5).should.deep.equal([]));
});
