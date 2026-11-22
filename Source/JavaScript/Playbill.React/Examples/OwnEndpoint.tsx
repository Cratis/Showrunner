// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Playbill, PlaybillProvider } from '@cratis/showrunner.playbill.react';

const withTenantHeader: typeof fetch = (input, init) => fetch(input, {
    ...init,
    headers: { ...init?.headers, 'X-Tenant': 'acme' }
});

export const OwnEndpoint = () => (
    <PlaybillProvider basePath='/api/acme/.cratis/playbill/v1' fetcher={withTenantHeader}>
        <Playbill />
    </PlaybillProvider>
);
