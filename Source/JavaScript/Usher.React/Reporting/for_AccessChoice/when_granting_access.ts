// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { AccessDuration } from '@cratis/showrunner.usher';
import { changeAccessChoice, initialAccessChoice, sharesIdentity } from '../AccessChoice.js';

describe('when granting access', () => {
    const granted = changeAccessChoice(initialAccessChoice(false), 'grantAccess', true);
    const triedToStopSharing = changeAccessChoice(granted, 'shareIdentity', false);

    it('should start without access', () => initialAccessChoice(false).grantAccess.should.be.false);
    it('should start with access for one day', () => initialAccessChoice(false).accessDuration.should.equal(AccessDuration.oneDay));
    it('should grant access', () => granted.grantAccess.should.be.true);
    it('should share who the reporter is', () => sharesIdentity(granted).should.be.true);
    it('should keep sharing who the reporter is when they try to stop', () => sharesIdentity(triedToStopSharing).should.be.true);
});
