// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { issueRepositoryFor } from '../issueRepositoryFor.js';

describe('when working out the repository', () => {
    it('should give one address to every version', () => issueRepositoryFor('https://github.com/Cratis/Direct', '1.0.0')!.should.equal('https://github.com/Cratis/Direct'));
    it('should ask a function for each version', () =>
        issueRepositoryFor(version => version.startsWith('1.') ? 'https://github.com/Cratis/Old' : 'https://github.com/Cratis/New', '2.0.0')!.should.equal('https://github.com/Cratis/New'));
    it('should leave the issues alone when nothing was given', () => (issueRepositoryFor(undefined, '1.0.0') === undefined).should.be.true);
    it('should leave the issues alone when the function has no answer', () => (issueRepositoryFor(() => undefined, '1.0.0') === undefined).should.be.true);
});
