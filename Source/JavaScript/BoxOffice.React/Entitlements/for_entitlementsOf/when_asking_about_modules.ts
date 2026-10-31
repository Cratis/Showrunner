// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ModuleAccess } from '@cratis/showrunner.boxoffice';
import { entitlementsFor, moduleStanding, summaryOf } from '../../given/boxOfficeData.js';

describe('when asking about modules', () => {
    const entitlements = entitlementsFor(summaryOf({
        modules: [
            moduleStanding('core', 'Core', ModuleAccess.full),
            moduleStanding('analytics', 'Analytics', ModuleAccess.trial, 9),
            moduleStanding('agents', 'Agents', ModuleAccess.preview, 4),
            moduleStanding('reports', 'Reports', ModuleAccess.none)
        ]
    }));

    it('should have a module in full', () => entitlements.has('core').should.be.true);
    it('should have a module on trial', () => entitlements.has('analytics').should.be.true);
    it('should not have a module only previewed', () => entitlements.has('agents').should.be.false);
    it('should not have a module without access', () => entitlements.has('reports').should.be.false);
    it('should not have a module the product does not declare', () => entitlements.has('unknown').should.be.false);
    it('should tell the access of a module', () => entitlements.accessTo('agents').should.equal(ModuleAccess.preview));
    it('should tell no access to a module the product does not declare', () => entitlements.accessTo('unknown').should.equal(ModuleAccess.none));
    it('should tell the standing of a module', () => entitlements.standingOf('analytics')!.name.should.equal('Analytics'));
    it('should tell nothing about a module the product does not declare', () => (entitlements.standingOf('unknown') === undefined).should.be.true);
});
