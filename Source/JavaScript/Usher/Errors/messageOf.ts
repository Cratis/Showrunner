// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ValidationResult } from '@cratis/arc/validation';

/**
 * Gets the message of a failed command or query result: its exception messages, otherwise the reason it was not
 * authorized, otherwise its validation messages, joined with new lines.
 * @param exceptionMessages The exception messages.
 * @param authorizationFailureReason The reason it was not authorized; empty when it was.
 * @param validationResults The validation results.
 * @returns The message; empty when the result carries none.
 */
export function messageOf(exceptionMessages: string[], authorizationFailureReason: string, validationResults: ValidationResult[]): string {
    if (exceptionMessages.length > 0) {
        return exceptionMessages.join('\n');
    }

    if (authorizationFailureReason) {
        return authorizationFailureReason;
    }

    return validationResults.map(result => result.message).join('\n');
}
