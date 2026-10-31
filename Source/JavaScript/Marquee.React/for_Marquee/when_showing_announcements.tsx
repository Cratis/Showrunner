// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { CratisComponentsProvider } from '@cratis/components';
import { Display, Importance } from '@cratis/showrunner.marquee';
import { act, cleanup, fireEvent, render, RenderResult } from '@testing-library/react';
import sinon from 'sinon';
import { a_marquee_backend } from '../given/a_marquee_backend.js';
import { anAnnouncement } from '../given/an_announcement.js';
import { Marquee, MarqueeProps } from '../Marquee.js';

describe('when showing announcements', () => {
    let backend: a_marquee_backend;
    let clock: sinon.SinonFakeTimers;
    let result: RenderResult;

    const show = (props: MarqueeProps = {}) => {
        result = render(<CratisComponentsProvider><Marquee {...props} /></CratisComponentsProvider>);
    };

    beforeEach(() => {
        clock = sinon.useFakeTimers({ now: new Date('2026-11-01T12:00:00Z'), toFake: ['Date', 'setInterval', 'clearInterval'] });
        backend = new a_marquee_backend();
    });

    afterEach(() => {
        cleanup();
        backend.restore();
        clock.restore();
    });

    describe('and a banner is active', () => {
        beforeEach(() => {
            backend.announcements = [anAnnouncement({ title: 'Down at six', display: Display.banner, importance: Importance.critical })];
            show();
        });

        it('should show it without any other code', () => result.getByRole('status').textContent!.should.contain('Down at six'));
        it('should not open a dialog', () => (result.queryByRole('dialog') === null).should.be.true);
    });

    describe('and a banner starts in the future', () => {
        beforeEach(() => {
            backend.announcements = [anAnnouncement({ title: 'Down at six', activeFrom: new Date('2026-11-01T12:01:30Z'), activeUntil: new Date('2026-11-01T12:10:00Z') })];
            show();
        });

        it('should not show it yet', () => result.getByRole('status').children.should.have.length(0));

        describe('and the clock is checked again after a minute', () => {
            beforeEach(() => act(() => { clock.tick(60_000); }));

            it('should still wait', () => result.getByRole('status').children.should.have.length(0));

            describe('and after another minute', () => {
                beforeEach(() => act(() => { clock.tick(60_000); }));

                it('should show it', () => result.getByRole('status').children.should.have.length(1));

                describe('and its window ends', () => {
                    beforeEach(() => act(() => { clock.tick(10 * 60_000); }));

                    it('should take it away', () => result.getByRole('status').children.should.have.length(0));
                });
            });
        });
    });

    describe('and a dialog shown once is closed', () => {
        beforeEach(() => {
            backend.announcements = [anAnnouncement({ id: undefined, title: 'A new module', display: Display.dialogOnce })];
            show();
            fireEvent.click(result.getByRole('button', { name: 'Got it' }));
        });

        it('should acknowledge it through the command', () => backend.acknowledged.should.deep.equal([backend.announcements[0].id.toString()]));
        it('should close it', () => (result.queryByRole('dialog') === null).should.be.true);
    });

    describe('and a dialog shown every session is closed', () => {
        beforeEach(() => {
            backend.announcements = [anAnnouncement({ title: 'Terms changed', display: Display.dialogEverySession })];
            show();
            fireEvent.click(result.getByRole('button', { name: 'Got it' }));
        });

        it('should not acknowledge it', () => backend.acknowledged.should.be.empty);
        it('should close it for the session', () => (result.queryByRole('dialog') === null).should.be.true);

        describe('and the announcements are observed again', () => {
            beforeEach(() => {
                backend.announcements = [...backend.announcements];
                act(() => { clock.tick(60_000); });
            });

            it('should not offer it again in this session', () => (result.queryByRole('dialog') === null).should.be.true);
        });
    });

    describe('and several dialogs are due', () => {
        beforeEach(() => {
            backend.announcements = [
                anAnnouncement({ title: 'A new module', display: Display.dialogOnce, importance: Importance.information }),
                anAnnouncement({ title: 'Security notice', display: Display.dialogOnce, importance: Importance.critical })
            ];
            show();
        });

        it('should open the most important first', () => result.getByRole('dialog').textContent!.should.contain('Security notice'));
        it('should offer to move on', () => result.getByRole('button', { name: 'Next' }).textContent!.should.equal('Next'));

        describe('and it is closed', () => {
            beforeEach(() => fireEvent.click(result.getByRole('button', { name: 'Next' })));

            it('should open the next', () => result.getByRole('dialog').textContent!.should.contain('A new module'));
            it('should finish on the last', () => result.getByRole('button', { name: 'Got it' }).textContent!.should.equal('Got it'));
        });
    });

    describe('and the banner and the dialog are replaced', () => {
        beforeEach(() => {
            backend.announcements = [
                anAnnouncement({ title: 'Banner one', display: Display.banner }),
                anAnnouncement({ title: 'Dialog one', display: Display.dialogOnce })
            ];
            show({
                banner: ({ announcements }) => <ul data-custom-banner>{announcements.map(_ => <li key={_.id.toString()}>{_.title}</li>)}</ul>,
                dialog: ({ announcement, onClose }) => <button data-custom-dialog onClick={onClose}>{announcement.title}</button>
            });
        });

        it('should draw the banners itself', () => result.container.querySelector('[data-custom-banner]')!.textContent!.should.equal('Banner one'));
        it('should draw the dialog itself', () => document.querySelector('[data-custom-dialog]')!.textContent!.should.equal('Dialog one'));
        it('should still acknowledge when the dialog it drew is closed', () => {
            fireEvent.click(document.querySelector('[data-custom-dialog]')!);
            backend.acknowledged.should.have.length(1);
        });
    });

    describe('and the texts are replaced', () => {
        beforeEach(() => {
            backend.announcements = [anAnnouncement({ display: Display.dialogOnce })];
            show({ labels: { acknowledge: 'Forstått' } });
        });

        it('should use them', () => result.getByRole('button', { name: 'Forstått' }).textContent!.should.equal('Forstått'));
    });
});
