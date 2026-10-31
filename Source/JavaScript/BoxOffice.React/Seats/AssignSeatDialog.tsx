// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { DialogResult } from '@cratis/arc.react/dialogs';
import { TextInput } from '@cratis/components/Common';
import { Dialog, DialogInitialFocus } from '@cratis/components/Dialogs';
import { useState } from 'react';
import { useBoxOfficeLabels } from '../Provider/BoxOfficeContext.js';
import { emptySeatPerson, isValidSeatPerson, SeatPersonInput } from './seatModels.js';

/**
 * Props for {@link AssignSeatDialog}.
 */
export interface AssignSeatDialogProps {
    /** Whether the dialog is open. */
    visible: boolean;

    /** Whether the seat is being assigned. */
    isBusy?: boolean;

    /** Called when the user confirms, with the person to give a seat. */
    onAssign: (person: SeatPersonInput) => void;

    /** Called when the user closes the dialog. */
    onClose: () => void;
}

/**
 * Asks who to give a seat.
 */
export const AssignSeatDialog = ({ visible, isBusy, onAssign, onClose }: AssignSeatDialogProps) => {
    const labels = useBoxOfficeLabels();
    const [person, setPerson] = useState<SeatPersonInput>(emptySeatPerson);
    const change = (field: keyof SeatPersonInput) => (value: string) => setPerson(current => ({ ...current, [field]: value }));

    return (
        <Dialog
            title={labels.seats.assignTitle}
            visible={visible}
            okLabel={labels.seats.assignConfirm}
            cancelLabel={labels.common.cancel}
            closeAriaLabel={labels.common.close}
            initialFocus={DialogInitialFocus.Content}
            isValid={isValidSeatPerson(person)}
            isBusy={isBusy}
            onConfirm={() => onAssign(person)}
            onClose={result => {
                if (result !== DialogResult.Ok) {
                    setPerson(emptySeatPerson);
                    onClose();
                }
            }}>
            <form className='boxoffice-seat-form' onSubmit={event => event.preventDefault()}>
                <label className='boxoffice-field'>
                    <span>{labels.seats.name}</span>
                    <TextInput value={person.name} onChange={change('name')} autoComplete='off' />
                </label>
                <label className='boxoffice-field'>
                    <span>{labels.seats.email}</span>
                    <TextInput type='email' value={person.email} onChange={change('email')} autoComplete='off' />
                </label>
                <label className='boxoffice-field'>
                    <span>{labels.seats.identityProvider}</span>
                    <TextInput value={person.identityProvider} onChange={change('identityProvider')} autoComplete='off' />
                </label>
                <label className='boxoffice-field'>
                    <span>{labels.seats.subject}</span>
                    <TextInput value={person.subject} onChange={change('subject')} autoComplete='off' />
                </label>
            </form>
        </Dialog>
    );
};
