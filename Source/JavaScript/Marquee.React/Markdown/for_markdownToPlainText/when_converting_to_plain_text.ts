// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { hasMarkdown, markdownToPlainText } from '../markdownText.js';

describe('when converting to plain text', () => {
    it('should keep plain text as it is', () => markdownToPlainText('Maintenance tonight').should.equal('Maintenance tonight'));
    it('should keep the text of a link and drop its address', () => markdownToPlainText('Maintenance tonight. [Read more here.](https://example.com/status)').should.equal('Maintenance tonight. Read more here.'));
    it('should keep a link whose address has parentheses whole', () => markdownToPlainText('See [the page](https://example.com/a_(b)) now').should.equal('See the page now'));
    it('should drop the markers of strong emphasis', () => markdownToPlainText('This is **important** news').should.equal('This is important news'));
    it('should drop the markers of emphasis', () => markdownToPlainText('This is *quite* new and _also_ this').should.equal('This is quite new and also this'));
    it('should drop the markers of code', () => markdownToPlainText('Run `yarn build` first').should.equal('Run yarn build first'));
    it('should leave the inside of code alone', () => markdownToPlainText('Compute `a*b*c` now').should.equal('Compute a*b*c now'));
    it('should not read underscores in a word as emphasis', () => markdownToPlainText('The snake_case_name stays').should.equal('The snake_case_name stays'));
    it('should keep the alternative text of an image', () => markdownToPlainText('Look ![the logo](https://example.com/logo.png) here').should.equal('Look the logo here'));
    it('should show the address of an autolink', () => markdownToPlainText('Visit <https://example.com> today').should.equal('Visit https://example.com today'));
    it('should drop html tags', () => markdownToPlainText('Hello <b>world</b><script>x</script>').should.equal('Hello worldx'));
    it('should unescape what was escaped', () => markdownToPlainText('Cost is 5\\*3 and \\[not a link\\]').should.equal('Cost is 5*3 and [not a link]'));
    it('should collapse white space', () => markdownToPlainText('  A   [b](https://x.example)\n c ').should.equal('A b c'));
    it('should give nothing for nothing', () => markdownToPlainText(undefined).should.equal(''));
    it('should give nothing for an empty text', () => markdownToPlainText('').should.equal(''));
});

describe('when telling whether there is markdown', () => {
    it('should be true for text', () => hasMarkdown('Hello').should.be.true);
    it('should be false for an empty text', () => hasMarkdown('').should.be.false);
    it('should be false for white space', () => hasMarkdown('  \n ').should.be.false);
    it('should be false for nothing', () => hasMarkdown(undefined).should.be.false);
    it('should be false for null', () => hasMarkdown(null).should.be.false);
});
