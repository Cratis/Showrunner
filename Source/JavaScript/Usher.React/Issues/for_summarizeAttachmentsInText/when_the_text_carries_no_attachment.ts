// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { summarizeAttachmentsInText } from '../summarizeAttachmentsInText.js';

describe('when the text carries no attachment', () => {
    const result = summarizeAttachmentsInText('Plain text with no attachment.');

    it('should be left unchanged', () => result.should.equal('Plain text with no attachment.'));
});
