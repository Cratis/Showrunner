// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { AccessState } from '@cratis/showrunner.usher';
import { AccessStatus, accessStatusOf } from '../accessStatus.js';
import { access } from '../given/an_issue.js';

describe('when deciding whether support may sign in', () => {
    const now = new Date(Date.UTC(2026, 9, 27, 12, 0));
    const later = new Date(Date.UTC(2026, 9, 28, 12, 0));
    const earlier = new Date(Date.UTC(2026, 9, 27, 11, 0));

    it('should not when there is no access', () => accessStatusOf(undefined, now).should.equal(AccessStatus.notGranted));
    it('should not when never granted', () => accessStatusOf(access(), now).should.equal(AccessStatus.notGranted));
    it('should while the grant lasts', () => accessStatusOf(access({ state: AccessState.granted, validUntil: later }), now).should.equal(AccessStatus.granted));
    it('should not once the grant has passed its end', () => accessStatusOf(access({ state: AccessState.granted, validUntil: earlier }), now).should.equal(AccessStatus.notGranted));
    it('should not once revoked', () => accessStatusOf(access({ state: AccessState.revoked, validUntil: later }), now).should.equal(AccessStatus.notGranted));
    it('should be requested when support asks', () => accessStatusOf(access({}, 'To reproduce it'), now).should.equal(AccessStatus.requested));
    it('should be granted rather than requested while the grant lasts', () => accessStatusOf(access({ state: AccessState.granted, validUntil: later }, 'To reproduce it'), now).should.equal(AccessStatus.granted));
});
