// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { isNotesAvailable } from '../notesVisibility.js';

describe('when deciding what renders', () => {
    it('should render for a signed in user while notes is on', () => isNotesAvailable(true, true).should.be.true);
    it('should not render for a user who is not signed in', () => isNotesAvailable(false, true).should.be.false);
    it('should not render while notes is off', () => isNotesAvailable(true, false).should.be.false);
    it('should not render until it is known whether notes is on', () => isNotesAvailable(true, undefined).should.be.false);
});
