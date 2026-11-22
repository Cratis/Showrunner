// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { linkIssueReferences } from '../linkIssueReferences.js';

const repository = 'https://github.com/Cratis/Studio';

describe('when linking issue references', () => {
    it('should link a bare reference to the repository of the notes', () =>
        linkIssueReferences('Fixed the crash #981', repository).should.equal('Fixed the crash [#981](https://github.com/Cratis/Studio/issues/981)'));

    it('should link a parenthesized reference', () =>
        linkIssueReferences('- Fixed pop-in (#991)', repository).should.equal('- Fixed pop-in ([#991](https://github.com/Cratis/Studio/issues/991))'));

    it('should link a qualified reference to the repository it names', () =>
        linkIssueReferences('Depends on Cratis/Chronicle#3568', repository).should.equal('Depends on [Cratis/Chronicle#3568](https://github.com/Cratis/Chronicle/issues/3568)'));

    it('should link a reference at the start of a line', () =>
        linkIssueReferences('#42 was the first fix', repository).should.equal('[#42](https://github.com/Cratis/Studio/issues/42) was the first fix'));

    it('should not mind a slash at the end of the repository', () =>
        linkIssueReferences('Fixed #7', `${repository}/`).should.equal('Fixed [#7](https://github.com/Cratis/Studio/issues/7)'));

    it('should leave a heading alone', () => {
        const markdown = '## Fixed\n### Details';
        linkIssueReferences(markdown, repository).should.equal(markdown);
    });

    it('should leave a reference that is already a link alone', () => {
        const markdown = 'See [#7](https://github.com/Cratis/Studio/issues/7)';
        linkIssueReferences(markdown, repository).should.equal(markdown);
    });

    it('should leave a fragment of an address alone', () => {
        const markdown = 'See https://example.com/page#12 for details';
        linkIssueReferences(markdown, repository).should.equal(markdown);
    });
});
