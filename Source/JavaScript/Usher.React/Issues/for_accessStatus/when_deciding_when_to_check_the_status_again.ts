// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { AccessState } from '@cratis/showrunner.usher';
import { AccessStatusRefreshInterval, nextAccessStatusCheckIn } from '../accessStatus.js';
import { access } from '../given/an_issue.js';

describe('when deciding when to check the status again', () => {
    const now = new Date(Date.UTC(2026, 9, 27, 12, 0));
    const inTenSeconds = new Date(now.getTime() + 10_000);
    const tomorrow = new Date(Date.UTC(2026, 9, 28, 12, 0));

    it('should check when a grant ends soon', () => nextAccessStatusCheckIn(access({ state: AccessState.granted, validUntil: inTenSeconds }), now).should.equal(10_000));
    it('should check within a minute when a grant ends later', () => nextAccessStatusCheckIn(access({ state: AccessState.granted, validUntil: tomorrow }), now).should.equal(AccessStatusRefreshInterval));
    it('should check within a minute when nothing is granted', () => nextAccessStatusCheckIn(access(), now).should.equal(AccessStatusRefreshInterval));
    it('should check within a minute when there is no access', () => nextAccessStatusCheckIn(undefined, now).should.equal(AccessStatusRefreshInterval));
});
