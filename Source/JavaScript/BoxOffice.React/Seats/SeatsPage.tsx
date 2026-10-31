// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { DialogButtons, DialogResult, useConfirmationDialog } from '@cratis/arc.react/dialogs';
import { toastCommandResult } from '@cratis/components/Notifications';
import { AssignSeat, CurrentRoster, ReleaseSeat, SeatHolder, SeatHolding } from '@cratis/showrunner.boxoffice';
import { useState } from 'react';
import { useEntitlementsContext } from '../Entitlements/EntitlementsContext.js';
import { format } from '../Labels/index.js';
import { useBoxOfficeContext } from '../Provider/BoxOfficeContext.js';
import { AssignSeatDialog } from './AssignSeatDialog.js';
import { personOf, seatRows, seatSummary, SeatPersonInput } from './seatModels.js';
import { SeatsView } from './SeatsView.js';

/**
 * The seats page: who holds the tenant's seats, with buttons to assign and release them. For the people who maintain
 * the product; the backend refuses everyone else. Render it inside a {@link BoxOfficeProvider}, the `DialogComponents`
 * registration and a `CratisComponentsProvider` with a toaster.
 */
export const SeatsPage = () => {
    const { labels, refresh } = useBoxOfficeContext();
    const entitlements = useEntitlementsContext();
    const [roster, performRoster] = CurrentRoster.use();
    const [assignSeat] = AssignSeat.use();
    const [releaseSeat] = ReleaseSeat.use();
    const [confirm] = useConfirmationDialog();
    const [isAssigning, setIsAssigning] = useState(false);
    const [isBusy, setIsBusy] = useState(false);

    const rows = seatRows(roster.isSuccess ? roster.data : undefined, entitlements.seats);
    const changed = () => {
        void performRoster();
        refresh();
    };

    const assign = async (input: SeatPersonInput) => {
        setIsBusy(true);
        try {
            assignSeat.holder = personOf(input);
            if (toastCommandResult(await assignSeat.execute(), { successTitle: labels.seats.assignConfirm })) {
                setIsAssigning(false);
                changed();
            }
        } finally {
            setIsBusy(false);
        }
    };

    const release = async (holding: SeatHolding) => {
        const answer = await confirm(
            labels.seats.releaseTitle,
            format(labels.seats.releaseMessage, { name: holding.holder.name }),
            DialogButtons.YesNo);
        if (answer !== DialogResult.Yes) {
            return;
        }

        const holder = new SeatHolder();
        holder.identityProvider = holding.holder.identityProvider;
        holder.subject = holding.holder.identityProviderSubject;
        releaseSeat.holder = holder;
        if (toastCommandResult(await releaseSeat.execute(), { showSuccess: false })) {
            changed();
        }
    };

    return (
        <>
            <SeatsView
                rows={rows}
                summary={seatSummary(rows, entitlements.seats)}
                isReadOnly={entitlements.isReadOnly}
                isLoading={roster.isPerforming && !roster.isSuccess}
                isUnauthorized={!roster.isAuthorized}
                hasFailed={!roster.isPerforming && roster.isAuthorized && !roster.isSuccess}
                onAssign={() => setIsAssigning(true)}
                onRelease={holding => void release(holding)} />
            <AssignSeatDialog
                visible={isAssigning}
                isBusy={isBusy}
                onAssign={input => void assign(input)}
                onClose={() => setIsAssigning(false)} />
        </>
    );
};
