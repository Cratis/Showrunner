// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ModuleAccess } from '@cratis/showrunner.boxoffice';
import { entitlementsFor, moduleStanding, summaryOf } from '../../given/boxOfficeData.js';
import { defaultBoxOfficeLabels } from '../../Labels/index.js';
import { requiresModuleModel } from '../requiresModuleModel.js';

describe('when asking about a module', () => {
    const entitlements = entitlementsFor(summaryOf({
        modules: [
            moduleStanding('core', 'Core', ModuleAccess.full),
            moduleStanding('agents', 'Agents', ModuleAccess.preview, 4),
            moduleStanding('analytics', 'Analytics', ModuleAccess.none)
        ]
    }));
    const has = requiresModuleModel(entitlements, 'core', defaultBoxOfficeLabels);
    const previewed = requiresModuleModel(entitlements, 'agents', defaultBoxOfficeLabels);
    const lacking = requiresModuleModel(entitlements, 'analytics', defaultBoxOfficeLabels);
    const unknown = requiresModuleModel(entitlements, 'reports', defaultBoxOfficeLabels);

    it('should have a module the tenant has', () => has.hasModule.should.be.true);
    it('should not offer to try a module the tenant has', () => has.canTry.should.be.false);
    it('should not have a module only previewed', () => previewed.hasModule.should.be.false);
    it('should say the module can be looked at', () => previewed.text.should.equal('Your organization can look at Agents, but not use it yet.'));
    it('should offer to try a module only previewed', () => previewed.canTry.should.be.true);
    it('should name the module in the title', () => lacking.title.should.equal('Analytics is not part of your plan'));
    it('should say the organization does not have it', () => lacking.text.should.equal('Your organization does not have Analytics.'));
    it('should name a module the product does not declare by its id', () => unknown.title.should.equal('reports is not part of your plan'));
});
