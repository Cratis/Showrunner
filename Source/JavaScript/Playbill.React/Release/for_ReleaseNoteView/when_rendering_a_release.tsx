// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { cleanup, render, RenderResult } from '@testing-library/react';
import { defaultPlaybillLabels } from '../../Labels/index.js';
import { aRelease } from '../../given/releases.js';
import { ReleaseNoteView, ReleaseNoteViewProps } from '../ReleaseNoteView.js';

describe('when rendering a release', () => {
    let result: RenderResult;

    const show = (props: Partial<ReleaseNoteViewProps> = {}) => {
        result = render(<ReleaseNoteView
            release={aRelease({
                version: '2.3.0',
                lead: 'Faster startup.',
                sections: [
                    { name: 'Added', markdown: '- A new **page**' },
                    { name: 'Fixed', markdown: '- A crash' },
                    { name: 'Highlights', markdown: '- Written another way' }
                ]
            })}
            labels={defaultPlaybillLabels}
            locale='en-US'
            {...props} />);
    };

    afterEach(() => cleanup());

    describe('and it is an ordinary release', () => {
        beforeEach(() => show());

        it('should name the version', () => result.getByRole('heading', { level: 2 }).textContent!.should.equal('2.3.0'));
        it('should say when it was released, in UTC', () => result.container.querySelector('time')!.textContent!.should.equal('Released March 2, 2026'));
        it('should keep the machine readable date', () => result.container.querySelector('time')!.getAttribute('datetime')!.should.equal('2026-03-02T10:00:00Z'));
        it('should show the lead', () => result.container.querySelector('.playbill-release__lead')!.textContent!.should.contain('Faster startup.'));
        it('should show a heading for each section, below the version', () => result.getAllByRole('heading', { level: 3 }).map(_ => _.textContent).should.deep.equal(['Added', 'Fixed', 'Highlights']));
        it('should mark a known section with its style', () => result.container.querySelector('.playbill-section--added')!.textContent!.should.contain('A new page'));
        it('should show an unknown heading as it was written', () => result.container.querySelector('.playbill-section--other .playbill-section__title')!.textContent!.should.equal('Highlights'));
        it('should not tag it', () => result.container.querySelectorAll('.playbill-tag').should.have.length(0));
    });

    describe('and it is the running version and has not been seen', () => {
        beforeEach(() => show({ isCurrent: true, isUnread: true }));

        it('should tag it as current', () => result.container.querySelector('.playbill-tag--current')!.textContent!.should.equal('Current'));
        it('should tag it as new', () => result.container.querySelector('.playbill-tag--unread')!.textContent!.should.equal('New'));
    });

    describe('and it has no lead and no date', () => {
        beforeEach(() => show({ release: aRelease({ lead: '', releasedAt: undefined }) }));

        it('should not draw a lead', () => (result.container.querySelector('.playbill-release__lead') === null).should.be.true);
        it('should not draw a date', () => (result.container.querySelector('time') === null).should.be.true);
    });

    describe('and it sits under a heading of level three', () => {
        beforeEach(() => show({ headingLevel: 3 }));

        it('should name the version one level down', () => result.getAllByRole('heading', { level: 3 })[0].textContent!.should.equal('2.3.0'));
        it('should put its sections one level further down', () => result.getAllByRole('heading', { level: 4 }).should.have.length(3));
    });

    describe('and its images are loaded from the folder of the release', () => {
        beforeEach(() => show({ release: aRelease({ lead: '![A hero shot](hero.png)' }), assetBase: '/.cratis/playbill/v1/releases/2.3.0' }));

        it('should resolve the image', () => result.container.querySelector('img')!.getAttribute('src')!.should.equal('/.cratis/playbill/v1/releases/2.3.0/hero.png'));
    });
});
