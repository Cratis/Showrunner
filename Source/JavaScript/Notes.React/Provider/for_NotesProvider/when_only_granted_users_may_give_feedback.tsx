// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { render, cleanup } from '@testing-library/react';
import sinon from 'sinon';
import { NotesButton } from '../../Buttons/NotesButton.js';
import { NotesApplication, stubAvailability } from '../../given/NotesApplication.js';

describe('when only granted users may give feedback', () => {
    afterEach(() => { cleanup(); sinon.restore(); });

    describe('and the user is granted', () => {
        it('should show the feedback button', () => {
            stubAvailability(true);
            const result = render(<NotesApplication><NotesButton /></NotesApplication>);
            result.getByRole('button', { name: 'Give feedback' }).tagName.should.equal('BUTTON');
        });
    });

    describe('and nobody granted the user', () => {
        it('should not show the feedback button, and give no hint that it exists', () => {
            stubAvailability(false);
            const result = render(<NotesApplication><NotesButton /></NotesApplication>);
            (result.queryByRole('button', { name: /feedback/i }) === null).should.equal(true);
        });
    });

    describe('and the user signs in after the first answer', () => {
        it('should ask again, once, because the first answer was given to nobody', () => {
            const perform = stubAvailability(false);
            const result = render(<NotesApplication signedIn={false}><NotesButton /></NotesApplication>);
            perform.called.should.equal(false);
            result.rerender(<NotesApplication signedIn><NotesButton /></NotesApplication>);
            perform.callCount.should.equal(1);
        });
    });

    describe('and the user is signed in from the start', () => {
        it('should not ask a second time', () => {
            const perform = stubAvailability(true);
            render(<NotesApplication><NotesButton /></NotesApplication>);
            perform.called.should.equal(false);
        });
    });
});
