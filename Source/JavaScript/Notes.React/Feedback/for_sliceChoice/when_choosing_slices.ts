// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { distinctSlices, isChosen, searchSlices, sliceKey, toggleSlice } from '../sliceChoice.js';

const changeOrder = { module: 'Orders', feature: 'Editing', slice: 'Change order' };
const orderList = { module: 'Orders', feature: 'Listing', slice: 'Order list' };
const pay = { module: 'Billing', feature: 'Invoices', slice: 'Pay' };
const none = { module: '', feature: '', slice: '' };

describe('when choosing slices', () => {
    it('should key a slice by its three parts', () => sliceKey(changeOrder).should.equal('Orders/Editing/Change order'));
    it('should drop repeats and empty locations', () => distinctSlices([changeOrder, none, orderList, { ...changeOrder }]).should.deep.equal([changeOrder, orderList]));
    it('should add a slice that is not chosen', () => toggleSlice([changeOrder], pay).should.deep.equal([changeOrder, pay]));
    it('should remove a slice that is chosen', () => toggleSlice([changeOrder, pay], { ...changeOrder }).should.deep.equal([pay]));
    it('should tell a chosen slice', () => isChosen([pay], { ...pay }).should.be.true);
    it('should tell a slice that is not chosen', () => isChosen([pay], changeOrder).should.be.false);
});

describe('when searching slices', () => {
    const known = [changeOrder, orderList, pay, { ...pay }];

    it('should find nothing for an empty query', () => searchSlices(known, '   ').should.be.empty);
    it('should find by part of a module', () => searchSlices(known, 'bill').should.deep.equal([pay]));
    it('should find without regard to case', () => searchSlices(known, 'ORDER LIST').should.deep.equal([orderList]));
    it('should need every word to match', () => searchSlices(known, 'orders editing').should.deep.equal([changeOrder]));
    it('should find nothing that does not match', () => searchSlices(known, 'shipping').should.be.empty);
});
