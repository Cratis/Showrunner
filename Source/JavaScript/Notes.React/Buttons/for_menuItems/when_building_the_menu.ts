// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { defaultNotesLabels } from '../../Labels/defaultNotesLabels.js';
import { menuItems, nextMenuIndex } from '../menuItems.js';

describe('when building the menu', () => {
    const items = menuItems(defaultNotesLabels, false);

    it('should offer slices, screen, pointing and my feedback in order', () => items.map(item => item.action).should.deep.equal(['slices', 'screen', 'component', 'myFeedback']));
    it('should label them', () => items.map(item => item.label).should.deep.equal(['About specific slices…', 'About this screen…', 'Point at something…', 'My feedback']));
    it('should switch nothing off while not pointing', () => items.every(item => !item.disabled).should.be.true);
    it('should switch pointing off while pointing', () => menuItems(defaultNotesLabels, true).find(item => item.action === 'component')!.disabled.should.be.true);
});

describe('when moving in the menu', () => {
    it('should go down', () => nextMenuIndex('ArrowDown', 0, 4)!.should.equal(1));
    it('should wrap down', () => nextMenuIndex('ArrowDown', 3, 4)!.should.equal(0));
    it('should go up', () => nextMenuIndex('ArrowUp', 2, 4)!.should.equal(1));
    it('should wrap up', () => nextMenuIndex('ArrowUp', 0, 4)!.should.equal(3));
    it('should go to the first and last', () => [nextMenuIndex('Home', 2, 4), nextMenuIndex('End', 1, 4)].should.deep.equal([0, 3]));
    it('should ignore other keys', () => (nextMenuIndex('a', 1, 4) === undefined).should.be.true);
});
