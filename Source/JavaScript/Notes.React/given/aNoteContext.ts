// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '@cratis/fundamentals';
import { ApplicationContextSnapshot } from '@cratis/arc.react';
import { SliceLocation } from '@cratis/arc/reflection';
import { NoteContext } from '@cratis/showrunner.notes';
import { buildNoteContext } from '../Context/noteContext.js';

export const orderSnapshot = (overrides: Partial<ApplicationContextSnapshot> = {}): ApplicationContextSnapshot => ({
    location: new SliceLocation('Orders', 'Editing', 'Change order'),
    activeSlices: [new SliceLocation('Orders', 'Editing', 'Change order'), new SliceLocation('Orders', 'Listing', 'Order list')],
    screen: { id: 'order-details', name: 'Order details', path: '/orders/42' },
    dialogs: [],
    contributions: {},
    ...overrides
});

export const aNoteContext = (overrides: Partial<ApplicationContextSnapshot> = {}): NoteContext => buildNoteContext({
    snapshot: orderSnapshot(overrides),
    route: '/orders/42',
    correlationId: Guid.create()
});
