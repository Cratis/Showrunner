// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ModuleAccess } from '@cratis/showrunner.boxoffice';
import { entitlementsFor, moduleStanding, summaryOf } from '../../given/boxOfficeData.js';
import { filterByModule } from '../moduleFilter.js';

const entries = [
    { name: 'Home' },
    { name: 'Analytics', module: 'analytics' },
    { name: 'Agents', module: 'agents' },
    { name: 'Reports', module: 'unknown' }
];

describe('when filtering navigation', () => {
    const entitlements = entitlementsFor(summaryOf({
        modules: [moduleStanding('analytics', 'Analytics', ModuleAccess.trial, 9), moduleStanding('agents', 'Agents', ModuleAccess.none)]
    }));
    const shown = filterByModule(entries, entitlements).map(_ => _.name);
    const whileLoading = filterByModule(entries, entitlementsFor(undefined)).map(_ => _.name);

    it('should keep what belongs to no module', () => shown.should.include('Home'));
    it('should keep what belongs to a module the tenant has', () => shown.should.include('Analytics'));
    it('should leave out what belongs to a module the tenant lacks', () => shown.should.not.include('Agents'));
    it('should leave out what belongs to a module the product does not declare', () => shown.should.not.include('Reports'));
    it('should keep the order', () => shown.should.deep.equal(['Home', 'Analytics']));
    it('should leave out what belongs to a module while loading', () => whileLoading.should.deep.equal(['Home']));
});
