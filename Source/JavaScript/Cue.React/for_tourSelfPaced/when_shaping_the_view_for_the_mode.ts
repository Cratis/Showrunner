// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { selfPacedViewFor } from '../tourSelfPaced.js';

describe('when shaping the view for the mode', () => {
    const content = { welcome: 'Welcome to the tour.' };

    describe('and the tour is playing narrated', () => {
        const view = selfPacedViewFor(false, content, 'welcome');

        it('should not be self-paced', () => view.isSelfPaced.should.be.false);
        it('should keep the media playback controls usable', () => view.isMediaPlaybackEnabled.should.be.true);
        it('should show the chapter timings', () => view.areTimingsShown.should.be.true);
        it('should show no markdown', () => view.markdown.should.equal(''));
    });

    describe('and the tour is being read self-paced', () => {
        const view = selfPacedViewFor(true, content, 'welcome');

        it('should be self-paced', () => view.isSelfPaced.should.be.true);
        it('should dim the media playback controls', () => view.isMediaPlaybackEnabled.should.be.false);
        it('should hide the chapter timings', () => view.areTimingsShown.should.be.false);
        it('should show the markdown for the chapter being read', () => view.markdown.should.equal('Welcome to the tour.'));
    });

    describe('and self-paced reading is asked for without any content to read', () => {
        const view = selfPacedViewFor(true, undefined, 'welcome');

        it('should stay out of self-paced mode', () => view.isSelfPaced.should.be.false);
        it('should keep the media playback controls usable', () => view.isMediaPlaybackEnabled.should.be.true);
        it('should show the chapter timings', () => view.areTimingsShown.should.be.true);
    });
});
