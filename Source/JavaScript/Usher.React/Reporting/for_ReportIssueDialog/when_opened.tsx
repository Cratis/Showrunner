// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { DialogContext, DialogContextContent } from '@cratis/arc.react/dialogs';
import { SliceLocation } from '@cratis/arc/reflection';
import { CratisComponentsProvider } from '@cratis/components';
import { Guid } from '@cratis/fundamentals';
import { cleanup, render, RenderResult } from '@testing-library/react';
import sinon from 'sinon';
import { defaultUsherContextValue, UsherContext } from '../../Provider/UsherContext.js';
import { ReportIssueDialog } from '../ReportIssueDialog.js';

describe('when opened', () => {
    let result: RenderResult;

    beforeEach(() => {
        result = render(
            <CratisComponentsProvider>
                <UsherContext.Provider value={{ ...defaultUsherContextValue, isProvided: true, applicationName: 'Ledger', applicationVersion: '4.12.1' }}>
                    <DialogContext.Provider value={new DialogContextContent({}, sinon.stub())}>
                        <ReportIssueDialog location={new SliceLocation('Billing', 'Invoices', 'Pay')} correlationIds={[Guid.create(), Guid.create(), Guid.create()]} />
                    </DialogContext.Provider>
                </UsherContext.Provider>
            </CratisComponentsProvider>
        );
    });

    afterEach(() => cleanup());

    it('should have the title', () => (result.queryByRole('heading', { name: 'Report a problem' }) === null).should.be.false);
    it('should show what is sent with the report', () => result.getByText('Billing › Invoices › Pay · Ledger 4.12.1 · 3 recent requests').textContent!.should.contain('3 recent requests'));
    it('should not grant access to begin with', () => (result.getByLabelText('Let support sign in as me to look into this') as HTMLInputElement).checked.should.be.false);
    it('should not share who the reporter is to begin with', () => (result.getByLabelText('Let support know who I am') as HTMLInputElement).checked.should.be.false);
    it('should not ask for how long to begin with', () => (result.queryByText('For how long') === null).should.be.true);
    it('should have the send button', () => (result.queryByRole('button', { name: 'Send report' }) === null).should.be.false);
});
