// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { CratisComponentsProvider } from '@cratis/components';
import { cleanup, fireEvent, render, RenderResult } from '@testing-library/react';
import sinon from 'sinon';
import { defaultUsherContextValue, UsherContext } from '../../Provider/UsherContext.js';
import { ReportButton, ReportButtonProps } from '../ReportButton.js';

const renderButton = (props: ReportButtonProps, reportIssue: sinon.SinonStub, reportError: sinon.SinonStub): RenderResult => render(
    <CratisComponentsProvider>
        <UsherContext.Provider value={{ ...defaultUsherContextValue, isProvided: true, reportIssue, reportError }}>
            <ReportButton {...props} />
        </UsherContext.Provider>
    </CratisComponentsProvider>
);

describe('when rendered', () => {
    afterEach(() => cleanup());

    describe('small', () => {
        let button: HTMLButtonElement;
        let reportIssue: sinon.SinonStub;

        beforeEach(() => {
            reportIssue = sinon.stub();
            const result = renderButton({ size: 'small' }, reportIssue, sinon.stub());
            button = result.getByRole('button') as HTMLButtonElement;
            fireEvent.click(button);
        });

        it('should be named for screen readers', () => button.getAttribute('aria-label')!.should.equal('Report a bug'));
        it('should show no text', () => (button.textContent ?? '').trim().should.equal(''));
        it('should have a tooltip', () => button.hasAttribute('data-cratis-tooltip-trigger').should.be.true);
        it('should open the report a problem dialog', () => reportIssue.calledOnce.should.be.true);
    });

    describe('large', () => {
        let button: HTMLButtonElement;

        beforeEach(() => {
            const result = renderButton({ size: 'large' }, sinon.stub(), sinon.stub());
            button = result.getByRole('button') as HTMLButtonElement;
        });

        it('should show the text', () => (button.textContent ?? '').should.contain('Report a bug'));
        it('should have no tooltip', () => button.hasAttribute('data-cratis-tooltip-trigger').should.be.false);
    });

    describe('with an error', () => {
        const error = new TypeError('Cannot read properties of undefined');
        let button: HTMLButtonElement;
        let reportError: sinon.SinonStub;

        beforeEach(() => {
            reportError = sinon.stub();
            const result = renderButton({ size: 'small', error }, sinon.stub(), reportError);
            button = result.getByRole('button') as HTMLButtonElement;
            fireEvent.click(button);
        });

        it('should be named for reporting the error', () => button.getAttribute('aria-label')!.should.equal('Report this error'));
        it('should open the report this error dialog with the error', () => reportError.firstCall.args[0].should.equal(error));
    });
});
