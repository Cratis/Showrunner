// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { errorDetailsFromError } from '../errorDetailsFromError';

describe('when building from something else that was thrown', () => {
    const details = errorDetailsFromError('The invoice could not be paid');
    const fromNothing = errorDetailsFromError(undefined);

    it('should have the text as the message', () => details.message.should.equal('The invoice could not be paid'));
    it('should have no type', () => details.type.should.equal(''));
    it('should have no stack frames', () => details.stackFrames.should.be.empty);
    it('should have an empty message for nothing', () => fromNothing.message.should.equal(''));
});
