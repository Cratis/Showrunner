// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { describe, it } from 'vitest';
import { projectToolbarPointToScreen } from '../tourGeometry.js';

describe('when projecting a toolbar point', () => {
    it('should keep the horizontal offset as it is', () => {
        projectToolbarPointToScreen(44, 0, 1000).x.should.equal(44);
    });

    it('should place a zero vertical offset on the vertical center', () => {
        projectToolbarPointToScreen(44, 0, 1000).y.should.equal(500);
    });

    it('should move with the offset from the vertical center', () => {
        projectToolbarPointToScreen(44, -22, 1000).y.should.equal(478);
        projectToolbarPointToScreen(44, 22, 1000).y.should.equal(522);
    });

    it('should follow the viewport so the same offset stays on the rail at any size', () => {
        (projectToolbarPointToScreen(44, -22, 800).y - 400).should.equal(projectToolbarPointToScreen(44, -22, 1600).y - 800);
    });
});
