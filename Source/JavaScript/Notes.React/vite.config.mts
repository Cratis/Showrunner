// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/// <reference types="vitest/config" />

import { defineConfig } from 'vitest/config';
import react from '@vitejs/plugin-react';

/* @ts-ignore TypeScript complains that the imported vite.config is not under rootDir, but it works at runtime */
import { createConfig } from '../../../vite.base.mjs';

const config = createConfig();
config.plugins.push(react());
config.test.environment = 'jsdom';
config.test.include = [... config.test.include, ...['**/for_*/when_*/**/*.tsx', '**/for_*/**/when_*.tsx']];

export default defineConfig(config);
