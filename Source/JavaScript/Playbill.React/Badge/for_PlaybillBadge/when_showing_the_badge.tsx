// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { cleanup, fireEvent, render, RenderResult } from '@testing-library/react';
import sinon from 'sinon';
import { a_playbill_backend } from '../../given/a_playbill_backend.js';
import { PlaybillBadge, PlaybillBadgeProps } from '../PlaybillBadge.js';

describe('when showing the badge', () => {
    let backend: a_playbill_backend;
    let result: RenderResult;

    const show = (props: PlaybillBadgeProps = {}) => {
        result = render(<PlaybillBadge {...props} />);
    };

    beforeEach(() => { backend = new a_playbill_backend(); });

    afterEach(() => {
        cleanup();
        backend.restore();
    });

    describe('and releases are unread', () => {
        beforeEach(() => {
            backend.unreadVersions = ['1.2.0', '1.1.0', '1.0.0'];
            show();
        });

        it('should show how many', () => result.container.querySelector('.playbill-badge__count')!.textContent!.should.equal('3'));
        it('should say it in words for assistive technology', () => result.getByRole('button', { name: 'Release notes, 3 new' }).tagName.should.equal('BUTTON'));
        it('should be marked as having something unread', () => (result.container.querySelector('.playbill-badge--unread') !== null).should.be.true);
    });

    describe('and nothing is unread', () => {
        beforeEach(() => show());

        it('should stay, so the history is one press away', () => result.getByRole('button', { name: 'Release notes' }).tagName.should.equal('BUTTON'));
        it('should show no count', () => (result.container.querySelector('.playbill-badge__count') === null).should.be.true);
    });

    describe('and nothing is unread and the badge is told to hide', () => {
        beforeEach(() => show({ hideWhenRead: true }));

        it('should draw nothing', () => result.container.children.should.have.length(0));
    });

    describe('and it is pressed', () => {
        const pressed = sinon.spy();
        beforeEach(() => {
            show({ onClick: pressed });
            fireEvent.click(result.getByRole('button'));
        });

        it('should tell the host', () => pressed.calledOnce.should.be.true);
    });

    describe('and the texts and the icon are replaced', () => {
        beforeEach(() => {
            backend.unreadVersions = ['1.0.0'];
            result = render(<PlaybillBadge labels={{ badgeUnread: '{count} nye utgivelser' }}><i data-custom-icon /></PlaybillBadge>);
        });

        it('should use the texts', () => result.getByRole('button', { name: '1 nye utgivelser' }).tagName.should.equal('BUTTON'));
        it('should use the icon', () => (result.container.querySelector('[data-custom-icon]') !== null).should.be.true);
    });
});
