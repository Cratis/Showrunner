// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useMemo, useReducer } from 'react';
import { AccessChoice, changeAccessChoice, initialAccessChoice, sharesIdentity } from './AccessChoice.js';

interface FieldChanged {
    readonly fieldName: string;
    readonly value: unknown;
}

const reduce = (choice: AccessChoice, change: FieldChanged) => changeAccessChoice(choice, change.fieldName, change.value);

/**
 * What {@link useAccessChoice} gives a command form.
 */
export interface AccessChoiceBinding {
    /** The reporter's {@link AccessChoice}. */
    readonly choice: AccessChoice;

    /** The handler for the form's `onFieldChange`. */
    readonly onFieldChange: (command: unknown, fieldName: string, oldValue: unknown, newValue: unknown) => void;

    /** The form's `currentValues`, which force sharing who the reporter is while access is granted. */
    readonly currentValues: { shareIdentity: boolean };

    /**
     * Gets the form's `initialValues` without `shareIdentity`: the form merges `initialValues` over `currentValues`, so
     * a `shareIdentity` there would win over the value the access choice forces.
     * @param values The initial values of the command.
     * @returns The values without `shareIdentity`.
     */
    readonly initialValues: <TValues extends { shareIdentity?: boolean }>(values: TValues) => Omit<TValues, 'shareIdentity'>;
}

const withoutShareIdentity = <TValues extends { shareIdentity?: boolean }>(values: TValues): Omit<TValues, 'shareIdentity'> => {
    const seed: Partial<TValues> = { ...values };
    delete seed.shareIdentity;
    return seed as Omit<TValues, 'shareIdentity'>;
};

/**
 * Follows the reporter's access choice in a command form with `shareIdentity`, `grantAccess` and `accessDuration`.
 * @param shareIdentityByDefault Whether to let support know who the reporter is unless they choose otherwise.
 * @returns The {@link AccessChoiceBinding}.
 */
export function useAccessChoice(shareIdentityByDefault: boolean): AccessChoiceBinding {
    const [choice, dispatch] = useReducer(reduce, shareIdentityByDefault, initialAccessChoice);
    const shareIdentity = sharesIdentity(choice);
    const currentValues = useMemo(() => ({ shareIdentity }), [shareIdentity]);
    const onFieldChange = (_command: unknown, fieldName: string, _oldValue: unknown, newValue: unknown) => dispatch({ fieldName, value: newValue });
    return { choice, onFieldChange, currentValues, initialValues: withoutShareIdentity };
}
