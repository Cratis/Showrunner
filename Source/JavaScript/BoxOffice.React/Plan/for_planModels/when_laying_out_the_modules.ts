// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ModuleAccess } from '@cratis/showrunner.boxoffice';
import { entitlementsFor, moduleStanding, summaryOf } from '../../given/boxOfficeData.js';
import { defaultBoxOfficeLabels } from '../../Labels/index.js';
import { moduleRows } from '../planModels.js';

describe('when laying out the modules', () => {
    const rows = moduleRows(entitlementsFor(summaryOf({
        modules: [
            moduleStanding('core', 'Core', ModuleAccess.full),
            moduleStanding('analytics', 'Analytics', ModuleAccess.trial, 9),
            moduleStanding('agents', 'Agents', ModuleAccess.preview, 4),
            moduleStanding('reports', 'Reports', ModuleAccess.none)
        ]
    })), defaultBoxOfficeLabels, 'en-US');

    it('should call included access included', () => rows[0].accessText.should.equal('Included'));
    it('should show included access as a success', () => rows[0].severity.should.equal('success'));
    it('should say no end for access that does not end', () => rows[0].untilText.should.equal(''));
    it('should call trial access a trial', () => rows[1].accessText.should.equal('Trial'));
    it('should say when a trial ends', () => rows[1].untilText.should.equal('until Nov 11, 2026'));
    it('should call preview access a preview', () => rows[2].accessText.should.equal('Preview'));
    it('should call missing access not included', () => rows[3].accessText.should.equal('Not included'));
    it('should offer to try a module the tenant lacks', () => rows[3].canTry.should.be.true);
    it('should offer to try a module only previewed', () => rows[2].canTry.should.be.true);
    it('should not offer to try a module the tenant has', () => rows[1].canTry.should.be.false);
});
