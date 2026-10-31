// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ModuleAccess, ReadOnlyReason } from '@cratis/showrunner.boxoffice';
import { entitlementsFor } from '../../given/boxOfficeData.js';

describe('when nothing is known', () => {
    const entitlements = entitlementsFor(undefined);

    it('should be loading', () => entitlements.isLoading.should.be.true);
    it('should be in the loading state', () => entitlements.state.should.equal('loading'));
    it('should not be read only', () => entitlements.isReadOnly.should.be.false);
    it('should not be read only for any reason', () => entitlements.readOnlyReason.should.equal(ReadOnlyReason.none));
    it('should have no module', () => entitlements.has('core').should.be.false);
    it('should give no access to a module', () => entitlements.accessTo('core').should.equal(ModuleAccess.none));
    it('should not offer the portal', () => entitlements.canOpenPortal.should.be.false);
});
