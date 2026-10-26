// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { CommandForm } from '@cratis/arc.react/commands';
import { CratisComponentsProvider } from '@cratis/components';
import { Guid } from '@cratis/fundamentals';
import { AccessDuration, ReportIssue } from '@cratis/showrunner.usher';
import { act, cleanup, fireEvent, render, RenderResult } from '@testing-library/react';
import { AccessFields } from '../AccessFields.js';
import { useAccessChoice } from '../useAccessChoice.js';

const Form = () => {
    const access = useAccessChoice(false);
    return (
        <CommandForm<ReportIssue>
            command={ReportIssue}
            initialValues={access.initialValues({ issueId: Guid.create(), summary: '', description: '', correlationIds: [], applicationVersion: '', shareIdentity: false, grantAccess: false, accessDuration: AccessDuration.oneDay })}
            currentValues={access.currentValues}
            onFieldChange={access.onFieldChange}>
            <AccessFields<ReportIssue> choice={access.choice} />
        </CommandForm>
    );
};

describe('when access is granted', () => {
    let result: RenderResult;
    let shareIdentity: HTMLInputElement;
    let wasDisabledBefore: boolean;

    beforeEach(async () => {
        result = render(<CratisComponentsProvider><Form /></CratisComponentsProvider>);
        wasDisabledBefore = (result.getByLabelText('Let support know who I am') as HTMLInputElement).disabled;
        await act(async () => {
            fireEvent.click(result.getByLabelText('Let support sign in as me to look into this'));
        });
        shareIdentity = result.getByLabelText('Let support know who I am') as HTMLInputElement;
    });

    afterEach(() => cleanup());

    it('should let the reporter choose before', () => wasDisabledBefore.should.be.false);
    it('should share who the reporter is', () => shareIdentity.checked.should.be.true);
    it('should not let the reporter stop sharing', () => shareIdentity.disabled.should.be.true);
    it('should ask for how long', () => (result.queryByText('For how long') === null).should.be.false);
    it('should say until when support can act as the reporter', () => result.container.textContent!.should.contain('Support will see who you are and can act as you until'));
    it('should point the checkbox at the notice', () => shareIdentity.getAttribute('aria-describedby')!.split(' ').should.have.lengthOf(2));

    describe('and then not', () => {
        beforeEach(async () => {
            await act(async () => {
                fireEvent.click(result.getByLabelText('Let support sign in as me to look into this'));
            });
            shareIdentity = result.getByLabelText('Let support know who I am') as HTMLInputElement;
        });

        it('should go back to what the reporter chose', () => shareIdentity.checked.should.be.false);
        it('should let the reporter choose again', () => shareIdentity.disabled.should.be.false);
        it('should no longer ask for how long', () => (result.queryByText('For how long') === null).should.be.true);
    });
});
