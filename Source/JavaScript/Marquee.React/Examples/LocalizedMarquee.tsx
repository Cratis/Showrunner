// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Marquee } from '@cratis/showrunner.marquee.react';

export const LocalizedMarquee = () => (
    <Marquee
        checkInterval={30_000}
        labels={{
            banners: 'Kunngjøringer',
            acknowledge: 'Forstått',
            next: 'Neste',
            close: 'Lukk',
            information: 'Informasjon',
            warning: 'Advarsel',
            critical: 'Kritisk'
        }} />
);
