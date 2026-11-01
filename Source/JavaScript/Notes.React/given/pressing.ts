// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { fireEvent, RenderResult, waitFor } from '@testing-library/react';

// Waits for the form to be valid, which it is asynchronously, and presses the button.
export const press = async (result: RenderResult, name: string, times = 1): Promise<void> => {
    await waitFor(() => (result.getByRole('button', { name }) as HTMLButtonElement).disabled.should.be.false);
    for (let press = 0; press < times; press++) {
        fireEvent.click(result.getByRole('button', { name }));
    }
};
