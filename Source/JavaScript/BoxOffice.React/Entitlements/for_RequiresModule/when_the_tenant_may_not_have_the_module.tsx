// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ModuleAccess } from '@cratis/showrunner.boxoffice';
import { cleanup, render, RenderResult } from '@testing-library/react';
import { BoxOfficeHarness } from '../../given/BoxOfficeHarness.js';
import { moduleStanding, summaryOf } from '../../given/boxOfficeData.js';
import { RequiresModule, RequiresModuleProps } from '../RequiresModule.js';

const lacking = summaryOf({ modules: [moduleStanding('agents', 'Agents', ModuleAccess.none)] });

const renderModule = (props: Partial<RequiresModuleProps>, canOpenPortal = true): RenderResult => render(
    <BoxOfficeHarness entitlements={{ ...lacking, canOpenPortal } as typeof lacking}>
        <RequiresModule module='agents' {...props}>
            <p>Agent runs</p>
        </RequiresModule>
    </BoxOfficeHarness>
);

describe('when the tenant may not have the module', () => {
    afterEach(() => cleanup());

    describe('and it is hidden', () => {
        let result: RenderResult;
        beforeEach(() => { result = renderModule({}); });

        it('should not show the content', () => (result.queryByText('Agent runs') === null).should.be.true);
        it('should not explain', () => (result.queryByText('Agents is not part of your plan') === null).should.be.true);
    });

    describe('and it is hidden with a fallback', () => {
        let result: RenderResult;
        beforeEach(() => { result = renderModule({ fallback: <p>Nothing here</p> }); });

        it('should show the fallback', () => (result.queryByText('Nothing here') !== null).should.be.true);
        it('should not show the content', () => (result.queryByText('Agent runs') === null).should.be.true);
    });

    describe('and it is explained', () => {
        let result: RenderResult;
        beforeEach(() => { result = renderModule({ mode: 'explain' }); });

        it('should say the module is not part of the plan', () => (result.queryByText('Agents is not part of your plan') !== null).should.be.true);
        it('should not show the content', () => (result.queryByText('Agent runs') === null).should.be.true);
        it('should offer to try it', () => (result.queryByRole('button', { name: 'Try it' }) !== null).should.be.true);
        it('should offer to buy it', () => (result.queryByRole('button', { name: 'Buy' }) !== null).should.be.true);
    });

    describe('and it is explained but the product has no portal', () => {
        let result: RenderResult;
        beforeEach(() => { result = renderModule({ mode: 'explain' }, false); });

        it('should still explain', () => (result.queryByText('Agents is not part of your plan') !== null).should.be.true);
        it('should offer nothing to buy', () => (result.queryByRole('button', { name: 'Buy' }) === null).should.be.true);
    });
});
