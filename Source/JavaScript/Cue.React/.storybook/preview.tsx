// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { CratisComponentsProvider } from '@cratis/components';
import '@cratis/components/tokens';
import '@cratis/components/styles';
import '@cratis/components/theme';
import type { Preview } from '@storybook/react-vite';
import '../styles.css';

const preview: Preview = {
    decorators: [
        Story => (
            <CratisComponentsProvider>
                <Story />
            </CratisComponentsProvider>
        )
    ]
};

export default preview;
