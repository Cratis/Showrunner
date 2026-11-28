// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { summarizeAttachmentsInText } from '../summarizeAttachmentsInText.js';

describe('when the text carries a markdown link', () => {
    const result = summarizeAttachmentsInText('See [crash.log](https://support.example.com/usher-attachments/abc.log) for details');

    it('should replace the link with a marker carrying the file name', () => result.should.equal('See 📎 crash.log for details'));
});
