// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { parseStackTrace } from '../parseStackTrace';

describe('when parsing nothing', () => {
    it('should have no frames without a stack trace', () => parseStackTrace().should.be.empty);
    it('should have no frames for an empty stack trace', () => parseStackTrace('').should.be.empty);
    it('should have no frames for a stack trace without frames', () => parseStackTrace('Error: Something failed').should.be.empty);
});
