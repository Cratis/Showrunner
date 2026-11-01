// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '@cratis/fundamentals';
import { PickedComponent } from '@cratis/showrunner.notes';
import { aNoteContext } from '../../given/aNoteContext.js';
import { propertySuggestions, propertyValueText } from '../propertySuggestions.js';

describe('when suggesting property changes', () => {
    const context = aNoteContext();
    const component = Object.assign(new PickedComponent(), { componentName: 'Button' });
    const noteId = Guid.create();
    const suggestions = propertySuggestions([
        { property: 'variant', from: 'solid', to: 'outline' },
        { property: 'disabled', from: undefined, to: true },
        { property: 'size', from: 3, to: 5 }
    ], context, component, noteId);

    it('should make one suggestion per changed property', () => suggestions.map(suggestion => suggestion.property).should.deep.equal(['variant', 'disabled', 'size']));
    it('should share the note id', () => suggestions.every(suggestion => suggestion.noteId === noteId).should.be.true);
    it('should carry the context and the component', () => (suggestions[0].context === context && suggestions[0].component === component).should.be.true);
    it('should carry the value before and after as text', () => suggestions[0].should.include({ currentValue: 'solid', suggestedValue: 'outline' }));
    it('should carry a property that was not set as empty text', () => suggestions[1].should.include({ currentValue: '', suggestedValue: 'true' }));
    it('should carry numbers as text', () => suggestions[2].should.include({ currentValue: '3', suggestedValue: '5' }));
    it('should make none for no changes', () => propertySuggestions([], context, component, noteId).should.be.empty);
    it('should turn nothing into empty text', () => propertyValueText(undefined).should.equal(''));
});
