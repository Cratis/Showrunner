// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { projectToScreen } from '../tourGeometry.js';

describe('when projecting a board point', () => {
    it('should sit at the pan offset for the board origin', () => {
        projectToScreen(0, 0, { zoom: 1, pan: { x: 32, y: 48 } }).should.eql({ x: 32, y: 48 });
    });

    it('should scale the board coordinate by the zoom', () => {
        projectToScreen(100, 200, { zoom: 0.5, pan: { x: 0, y: 0 } }).should.eql({ x: 50, y: 100 });
    });

    it('should combine the zoom and the pan', () => {
        projectToScreen(100, 100, { zoom: 2, pan: { x: -50, y: 10 } }).should.eql({ x: 150, y: 210 });
    });
});
