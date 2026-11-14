// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { resolveTourMedia } from '../tourSelfPaced.js';

describe('when resolving media references', () => {
    const mediaUrls = { 'media/board.png': '/assets/board-3f2a.png' };

    it('should resolve a reference to media that exists', () => {
        resolveTourMedia('Look: ![The board](media/board.png)', mediaUrls)
            .should.equal('Look: ![The board](/assets/board-3f2a.png)');
    });

    it('should resolve a reference written with a leading dot-slash', () => {
        resolveTourMedia('![The board](./media/board.png)', mediaUrls)
            .should.equal('![The board](/assets/board-3f2a.png)');
    });

    it('should resolve a reference carrying a title', () => {
        resolveTourMedia('![The board](media/board.png "The board")', mediaUrls)
            .should.equal('![The board](/assets/board-3f2a.png "The board")');
    });

    it('should leave a reference to media that has not been produced yet', () => {
        resolveTourMedia('![A missing shot](media/missing.png)', mediaUrls)
            .should.equal('![A missing shot](media/missing.png)');
    });

    it('should leave an ordinary link alone', () => {
        resolveTourMedia('[the docs](media/board.png)', mediaUrls)
            .should.equal('[the docs](media/board.png)');
    });

    it('should resolve every reference in a chapter', () => {
        resolveTourMedia('![One](media/board.png) and ![Two](media/board.png)', mediaUrls)
            .should.equal('![One](/assets/board-3f2a.png) and ![Two](/assets/board-3f2a.png)');
    });
});
