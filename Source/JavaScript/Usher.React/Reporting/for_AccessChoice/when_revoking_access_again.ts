// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { changeAccessChoice, initialAccessChoice, sharesIdentity } from '../AccessChoice.js';

describe('when revoking access again', () => {
    const notSharing = changeAccessChoice(changeAccessChoice(initialAccessChoice(false), 'grantAccess', true), 'grantAccess', false);
    const sharing = changeAccessChoice(changeAccessChoice(initialAccessChoice(true), 'grantAccess', true), 'grantAccess', false);

    it('should no longer grant access', () => notSharing.grantAccess.should.be.false);
    it('should go back to not sharing when the reporter had not chosen to', () => sharesIdentity(notSharing).should.be.false);
    it('should go back to sharing when the reporter had chosen to', () => sharesIdentity(sharing).should.be.true);
});
