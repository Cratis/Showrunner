// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Arc, useContributeApplicationContext } from '@cratis/arc.react';
import { DialogComponents } from '@cratis/arc.react/dialogs';
import { IdentityProviderContext } from '@cratis/arc.react/identity';
import { CratisComponentsProvider } from '@cratis/components';
import { BusyIndicatorDialog, ConfirmationDialog } from '@cratis/components/Dialogs';
import { QueryResultWithState } from '@cratis/arc/queries';
import { Availability, NotesAvailability } from '@cratis/showrunner.notes';
import { ReactNode } from 'react';
import sinon from 'sinon';
import { NotesProvider, NotesProviderProps } from '../Provider/NotesProvider.js';

export interface NotesApplicationProps extends NotesProviderProps {
    signedIn?: boolean;
    children?: ReactNode;
}

export const stubAvailability = (isEnabled: boolean) =>
    sinon.stub(Availability, 'use').returns([QueryResultWithState.empty(Object.assign(new NotesAvailability(), { isEnabled })), (() => undefined) as never, (() => undefined) as never]);

const Screen = () => {
    useContributeApplicationContext({ screen: { id: 'order-details', name: 'Order details', path: '/orders/42' }, values: { modelReferences: { eventModel: 'orders-v3' } } });
    return null;
};

export const NotesApplication = ({ signedIn = true, children, ...provider }: NotesApplicationProps) => (
    <CratisComponentsProvider toaster>
        <Arc sliceMarkers>
            <IdentityProviderContext.Provider value={{
                identity: { id: 'u1', name: 'Sample User', roles: [], details: {}, isSet: signedIn, isInRole: () => false, refresh: () => Promise.reject(new Error('not used')) },
                isLoading: false,
                clearIdentity: () => undefined
            }}>
                <DialogComponents confirmation={ConfirmationDialog} busyIndicator={BusyIndicatorDialog}>
                    <NotesProvider {...provider}>
                        <Screen />
                        {children}
                    </NotesProvider>
                </DialogComponents>
            </IdentityProviderContext.Provider>
        </Arc>
    </CratisComponentsProvider>
);
