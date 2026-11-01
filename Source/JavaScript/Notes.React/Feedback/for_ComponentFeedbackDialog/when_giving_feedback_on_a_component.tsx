// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { DialogResult } from '@cratis/arc.react/dialogs';
import { Button } from '@cratis/components/Common';
import { describeComponentElement, PickedComponent } from '@cratis/components/ComponentPicker';
import { GiveNoteOnComponent, NoteKind, SuggestComponentPropertyChange } from '@cratis/showrunner.notes';
import { cleanup, fireEvent, render, RenderResult, waitFor } from '@testing-library/react';
import sinon from 'sinon';
import { aNoteContext } from '../../given/aNoteContext.js';
import { stubExecute } from '../../given/executing.js';
import { NotesHarness } from '../../given/NotesHarness.js';
import { press } from '../../given/pressing.js';
import { ComponentFeedbackDialog } from '../ComponentFeedbackDialog.js';

const pickedButton = (host: HTMLElement): PickedComponent => {
    const element = host.querySelector('[data-cratis-component="Button"]')!;
    const marked = host.querySelector('[data-cratis-module]')!;
    marked.setAttribute('data-cratis-module', 'Orders');
    marked.setAttribute('data-cratis-feature', 'Editing');
    marked.setAttribute('data-cratis-slice', 'Change order');
    return describeComponentElement(element, null, el => {
        const slice = el.closest('[data-cratis-module]');
        return slice ? { slice: { module: slice.getAttribute('data-cratis-module'), feature: slice.getAttribute('data-cratis-feature'), slice: slice.getAttribute('data-cratis-slice') } } : undefined;
    });
};

describe('when giving feedback on a component', () => {
    const context = aNoteContext();
    let host: HTMLElement;
    let picked: PickedComponent;
    let page: RenderResult;

    beforeEach(() => {
        page = render(
            <NotesHarness>
                <div data-cratis-module=''><Button id='save' variant='solid' label='Save' /></div>
            </NotesHarness>
        );
        host = page.container;
        picked = pickedButton(host);
    });

    afterEach(() => { cleanup(); sinon.restore(); });

    describe('and describing it', () => {
        let execution: ReturnType<typeof stubExecute>;
        let closeDialog: sinon.SinonStub;
        let dialog: RenderResult;

        beforeEach(async () => {
            execution = stubExecute(GiveNoteOnComponent);
            closeDialog = sinon.stub();
            dialog = render(<NotesHarness closeDialog={closeDialog}><ComponentFeedbackDialog context={context} picked={picked} /></NotesHarness>);
            fireEvent.click(dialog.getByLabelText('Question'));
            fireEvent.change(dialog.getByLabelText('What do you think?'), { target: { value: 'What does this button save?' } });
            await press(dialog, 'Send feedback');
            await waitFor(() => closeDialog.called.should.be.true);
        });

        it('should keep the component highlighted while the dialog is open', () => (document.querySelector('[data-cratis-kind="highlight"]') !== null).should.be.true);
        it('should send the picked component', () => (execution.executions[0].component as { componentName: string }).componentName.should.equal('Button'));
        it('should send the slice the component belongs to', () => (execution.executions[0].component as { location: { slice: string } }).location.slice.should.equal('Change order'));
        it('should send the context', () => (execution.executions[0].context === context).should.be.true);
        it('should send the kind', () => execution.executions[0].kind!.should.equal(NoteKind.question));
        it('should send the text', () => execution.executions[0].text!.should.equal('What does this button save?'));
        it('should close the dialog', () => closeDialog.firstCall.args[0].should.equal(DialogResult.Ok));
    });

    describe('and changing it', () => {
        let execution: ReturnType<typeof stubExecute>;
        let closeDialog: sinon.SinonStub;
        let dialog: RenderResult;
        let variantBefore: string | null;

        beforeEach(async () => {
            execution = stubExecute(SuggestComponentPropertyChange);
            closeDialog = sinon.stub();
            variantBefore = host.querySelector('[data-cratis-component="Button"]')!.getAttribute('data-variant');
            dialog = render(<NotesHarness closeDialog={closeDialog}><ComponentFeedbackDialog context={context} picked={picked} initialMode='change' /></NotesHarness>);
        });

        it('should offer the properties of the component', () => picked.presentationProperties.length.should.be.greaterThan(0));

        describe('and sending the changes', () => {
            let changed: string[];

            beforeEach(async () => {
                const variant = dialog.getByLabelText('Variant') as HTMLSelectElement | HTMLInputElement;
                const options = picked.presentationProperties.find(property => property.name === 'variant')?.values ?? [];
                const to = String(options.find(option => String(option) !== String(picked.presentationProperties.find(property => property.name === 'variant')?.value)));
                fireEvent.change(variant, { target: { value: to } });
                changed = ['variant'];
                await press(dialog, 'Send changes');
                await waitFor(() => closeDialog.called.should.be.true);
            });

            it('should send one suggestion per changed property', () => execution.stub.callCount.should.equal(changed.length));
            it('should name the property', () => execution.executions[0].property!.should.equal('variant'));
            it('should send the value before as text', () => execution.executions[0].currentValue!.should.equal(String(picked.presentationProperties.find(property => property.name === 'variant')!.value)));
            it('should send the component and the context', () => ((execution.executions[0].context === context) && (execution.executions[0].component as { componentName: string }).componentName === 'Button').should.be.true);
            it('should close the dialog', () => closeDialog.firstCall.args[0].should.equal(DialogResult.Ok));
        });

        describe('and closing without sending', () => {
            beforeEach(() => {
                const variant = dialog.getByLabelText('Variant') as HTMLSelectElement;
                const options = picked.presentationProperties.find(property => property.name === 'variant')?.values ?? [];
                fireEvent.change(variant, { target: { value: String(options.find(option => String(option) !== String(picked.presentationProperties.find(property => property.name === 'variant')?.value))) } });
            });

            it('should preview the change', () => host.querySelector('[data-cratis-component="Button"]')!.getAttribute('data-variant')!.should.not.equal(variantBefore));

            it('should revert the preview', () => {
                dialog.unmount();
                host.querySelector('[data-cratis-component="Button"]')!.getAttribute('data-variant')!.should.equal(variantBefore);
            });
        });
    });
});
