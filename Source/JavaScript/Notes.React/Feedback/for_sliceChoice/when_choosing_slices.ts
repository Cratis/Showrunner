// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { distinctSlices, isChosen, searchSlices, sliceKey, toggleSlice, withParentLevels } from '../sliceChoice.js';

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

describe('when offering the broader levels', () => {
    const module = { module: 'Orders', feature: '', slice: '' };
    const feature = { module: 'Orders', feature: 'Editing', slice: '' };

    it('should offer the module and the feature before the slice in them', () => withParentLevels([changeOrder]).should.deep.equal([module, feature, changeOrder]));
    it('should offer a shared module once', () => withParentLevels([changeOrder, orderList]).should.deep.equal([module, feature, changeOrder, { module: 'Orders', feature: 'Listing', slice: '' }, orderList]));
    it('should keep a location that is only a module', () => withParentLevels([module]).should.deep.equal([module]));
    it('should keep a location that is only a feature', () => withParentLevels([feature]).should.deep.equal([module, feature]));
    it('should keep a location that has no module as it is', () => withParentLevels([{ module: '', feature: 'Editing', slice: 'Change order' }]).should.deep.equal([{ module: '', feature: 'Editing', slice: 'Change order' }]));
    it('should offer nothing for an empty location', () => withParentLevels([none]).should.be.empty);
    it('should find a whole module by searching for it', () => searchSlices(withParentLevels([changeOrder, pay]), 'billing').should.deep.equal([{ module: 'Billing', feature: '', slice: '' }, { module: 'Billing', feature: 'Invoices', slice: '' }, pay]));
});
