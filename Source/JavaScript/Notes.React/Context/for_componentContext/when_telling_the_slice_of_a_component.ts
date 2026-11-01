// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { describeComponentElement } from '@cratis/components/ComponentPicker';
import { SliceLocation } from '@cratis/arc/reflection';
import { resolveComponentContext, sliceMarkedOn, toNotePickedComponent } from '../componentContext.js';

const build = (html: string): HTMLElement => {
    const host = document.createElement('div');
    host.innerHTML = html;
    document.body.appendChild(host);
    return host;
};

describe('when telling the slice of a component', () => {
    let host: HTMLElement;
    let button: Element;

    beforeEach(() => {
        host = build(`
            <div data-cratis-module="Orders" data-cratis-feature="Editing" data-cratis-slice="Change order" style="display: contents">
                <form data-cratis-component="Form" id="order-form">
                    <button data-cratis-component="Button" data-cratis-component-id="save" id="save">Save</button>
                </form>
            </div>`);
        button = host.querySelector('#save')!;
    });

    afterEach(() => host.remove());

    it('should read the closest slice marker', () => sliceMarkedOn(button)!.should.deep.equal({ module: 'Orders', feature: 'Editing', slice: 'Change order' }));
    it('should resolve it as the context of the component', () => resolveComponentContext(button)!.should.deep.equal({ slice: { module: 'Orders', feature: 'Editing', slice: 'Change order' } }));

    describe('and the picker resolved it', () => {
        const noteComponent = () => toNotePickedComponent(describeComponentElement(button, null, resolveComponentContext), new SliceLocation('Other', 'Focused', 'Slice'));

        it('should name the component', () => noteComponent().componentName.should.equal('Button'));
        it('should carry the id of the component', () => noteComponent().componentId.should.equal('save'));
        it('should have no part for the component as a whole', () => noteComponent().part.should.equal(''));
        it('should carry the components it sits in, nearest first', () => noteComponent().ancestors.should.deep.equal(['Form']));
        it('should use the slice of the marker rather than the focused location', () => noteComponent().location.should.include({ module: 'Orders', feature: 'Editing', slice: 'Change order' }));
    });

    describe('and there are no slice markers', () => {
        beforeEach(() => {
            host.remove();
            host = build('<form data-cratis-component="Form"><button data-cratis-component="Button" id="plain">Save</button></form>');
            button = host.querySelector('#plain')!;
        });

        it('should have no marker', () => (sliceMarkedOn(button) === undefined).should.be.true);
        it('should add no context', () => (resolveComponentContext(button) === undefined).should.be.true);
        it('should fall back to the focused location', () =>
            toNotePickedComponent(describeComponentElement(button, null, resolveComponentContext), new SliceLocation('Other', 'Focused', 'Slice')).location.should.include({ module: 'Other', feature: 'Focused', slice: 'Slice' }));
    });
});
