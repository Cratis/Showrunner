// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Playbill } from '@cratis/showrunner.playbill.react';

export const LocalizedPlaybill = () => (
    <Playbill
        locale='nb-NO'
        labels={{
            releases: 'Utgivelser',
            released: 'Utgitt',
            unread: 'Ny',
            current: 'Gjeldende',
            added: 'Nytt',
            changed: 'Endret',
            fixed: 'Rettet',
            loading: 'Henter utgivelsesnotater…'
        }} />
);
