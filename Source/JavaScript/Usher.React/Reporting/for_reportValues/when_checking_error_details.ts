// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { isErrorDetails } from '../reportValues.js';

describe('when checking error details', () => {
    const valid = () => ({ message: 'Failed', type: 'Error', correlationIds: [], stackFrames: [], location: { module: 'Billing' } });
    const without = (member: string) => {
        const candidate: Record<string, unknown> = valid();
        delete candidate[member];
        return candidate;
    };
    const with_ = (member: string, value: unknown) => ({ ...valid(), [member]: value });

    it('should not take null for error details', () => isErrorDetails(null).should.be.false);
    it('should not take undefined for error details', () => isErrorDetails(undefined).should.be.false);
    it('should not take a string for error details', () => isErrorDetails('Failed').should.be.false);
    it('should not take a number for error details', () => isErrorDetails(42).should.be.false);
    it('should not take an array for error details', () => isErrorDetails([]).should.be.false);
    it('should not take an error for error details', () => isErrorDetails(new Error('Failed')).should.be.false);
    it('should not take a type error for error details', () => isErrorDetails(new TypeError('Failed')).should.be.false);
    it('should not take an empty object for error details', () => isErrorDetails({}).should.be.false);
    it('should not take error details without a message', () => isErrorDetails(without('message')).should.be.false);
    it('should not take error details without a type', () => isErrorDetails(without('type')).should.be.false);
    it('should not take error details without correlation ids', () => isErrorDetails(without('correlationIds')).should.be.false);
    it('should not take error details without stack frames', () => isErrorDetails(without('stackFrames')).should.be.false);
    it('should not take error details without a location', () => isErrorDetails(without('location')).should.be.false);
    it('should not take a message that is not text', () => isErrorDetails(with_('message', 1)).should.be.false);
    it('should not take a type that is not text', () => isErrorDetails(with_('type', {})).should.be.false);
    it('should not take correlation ids that are an object', () => isErrorDetails(with_('correlationIds', {})).should.be.false);
    it('should not take correlation ids that are text', () => isErrorDetails(with_('correlationIds', 'abc')).should.be.false);
    it('should not take stack frames that are an object', () => isErrorDetails(with_('stackFrames', {})).should.be.false);
    it('should not take stack frames that are text', () => isErrorDetails(with_('stackFrames', 'abc')).should.be.false);
    it('should not take a location that is null', () => isErrorDetails(with_('location', null)).should.be.false);
    it('should not take a location that is text', () => isErrorDetails(with_('location', 'Billing')).should.be.false);
    it('should take valid error details', () => isErrorDetails(valid()).should.be.true);
    it('should take valid error details with more members', () => isErrorDetails(with_('extra', 1)).should.be.true);
    it('should take a location that is an empty object', () => isErrorDetails(with_('location', {})).should.be.true);
});
