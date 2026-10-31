// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ModuleAccess } from '@cratis/showrunner.boxoffice';
import { cleanup, render } from '@testing-library/react';
import { BoxOfficeHarness } from '../../given/BoxOfficeHarness.js';
import { moduleStanding, summaryOf } from '../../given/boxOfficeData.js';
import { EntitlementsContext } from '../EntitlementsContext.js';
import { entitlementsOf } from '../entitlementModel.js';
import { RequiresModule } from '../RequiresModule.js';

describe('when the tenant may have the module', () => {
    afterEach(() => cleanup());

    describe('in full', () => {
        const result = () => render(
            <BoxOfficeHarness entitlements={summaryOf({ modules: [moduleStanding('agents', 'Agents', ModuleAccess.full)] })}>
                <RequiresModule module='agents' mode='explain'><p>Agent runs</p></RequiresModule>
            </BoxOfficeHarness>
        );

        it('should show the content', () => (result().queryByText('Agent runs') !== null).should.be.true);
    });

    describe('on trial', () => {
        const result = () => render(
            <BoxOfficeHarness entitlements={summaryOf({ modules: [moduleStanding('agents', 'Agents', ModuleAccess.trial, 5)] })}>
                <RequiresModule module='agents'><p>Agent runs</p></RequiresModule>
            </BoxOfficeHarness>
        );

        it('should show the content', () => (result().queryByText('Agent runs') !== null).should.be.true);
    });

    describe('while nothing is known yet', () => {
        const result = () => render(
            <EntitlementsContext.Provider value={entitlementsOf(undefined)}>
                <RequiresModule module='agents' mode='explain' fallback={<p>Nothing here</p>}><p>Agent runs</p></RequiresModule>
            </EntitlementsContext.Provider>
        );

        it('should not show the content', () => (result().queryByText('Agent runs') === null).should.be.true);
        it('should not show the fallback', () => (result().queryByText('Nothing here') === null).should.be.true);
        it('should not explain', () => (result().queryByText('agents is not part of your plan') === null).should.be.true);
    });
});
