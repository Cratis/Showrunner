// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { CommandResult } from '@cratis/arc/commands';
import { errorDetailsFromCommandResult } from '../errorDetailsFromCommandResult';

const failed = (authorizationFailureReason: string, validationMessages: string[]) => new CommandResult({
    correlationId: '3f2504e0-4f89-11d3-9a0c-0305e82c3301',
    isSuccess: false,
    isAuthorized: authorizationFailureReason.length === 0,
    isValid: validationMessages.length === 0,
    hasExceptions: false,
    validationResults: validationMessages.map(message => ({ severity: 2, message, members: [], state: {} })),
    exceptionMessages: [],
    exceptionStackTrace: '',
    authorizationFailureReason,
    response: undefined
}, Object, false);

describe('when building from a command without exceptions', () => {
    const unauthorized = errorDetailsFromCommandResult(failed('Only the reporter can do this', []));
    const invalid = errorDetailsFromCommandResult(failed('', ['Summary is required', 'Description is required']));

    it('should have the authorization failure as the message', () => unauthorized.message.should.equal('Only the reporter can do this'));
    it('should have the validation messages as the message', () => invalid.message.should.equal('Summary is required\nDescription is required'));
    it('should have no stack frames', () => invalid.stackFrames.should.be.empty);
});
