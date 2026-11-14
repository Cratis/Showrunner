// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { CHROME_INSETS, projectEditorPointToScreen, unobstructedCenter } from '../tourGeometry.js';

describe('when projecting an editor point', () => {
    it('should center on the middle of the viewport the chrome leaves open', () => {
        unobstructedCenter(1600, 1000).should.eql({
            x: 800 - (CHROME_INSETS.right - CHROME_INSETS.left) / 2,
            y: 500 - CHROME_INSETS.bottom / 2,
        });
    });

    it('should place the panel center offset of zero on the unobstructed center', () => {
        projectEditorPointToScreen(0, 0, 1600, 1000).should.eql(unobstructedCenter(1600, 1000));
    });

    it('should move with the offsets from the panel center', () => {
        const center = unobstructedCenter(1600, 1000);
        projectEditorPointToScreen(50, -30, 1600, 1000).should.eql({ x: center.x + 50, y: center.y - 30 });
    });

    it('should follow the viewport so the same offsets stay on the panel at any size', () => {
        const small = projectEditorPointToScreen(0, 239, 1280, 800);
        const large = projectEditorPointToScreen(0, 239, 3200, 1600);
        (small.x - 1280 / 2).should.equal(large.x - 3200 / 2);
        (small.y - 800 / 2).should.equal(large.y - 1600 / 2);
    });
});
