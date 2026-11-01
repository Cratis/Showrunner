// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Button, IconButton } from '@cratis/components/Common';
import { DialogHeaderContext } from '@cratis/components/Dialogs';
import { KeyboardEvent, useEffect, useId, useRef, useState } from 'react';
import { ChevronIcon, FeedbackIcon } from '../Icons.js';
import { useNotesContext } from '../Provider/NotesContext.js';
import { menuItems, nextMenuIndex, NotesMenuAction } from './menuItems.js';

/**
 * Props for {@link NotesButton}.
 */
export interface NotesButtonProps {
    /** `large` shows the icon and the text; `small` shows the icon, with the text as tooltip and accessible name. Defaults to `large`. */
    size?: 'large' | 'small';

    /** The dialog the button is in; the feedback is about the dialog and its slice when given. */
    dialog?: DialogHeaderContext;

    /** Additional class name for the button. */
    className?: string;
}

/**
 * The feedback button, a split button. Its main action gives feedback on what the user is looking at; its drop-down offers
 * feedback on specific slices, on this screen, on a component the user points at, and the user's own feedback. It
 * renders nothing unless the user is signed in and Notes is switched on. Place it in any top bar.
 */
export const NotesButton = ({ size = 'large', dialog, className }: NotesButtonProps) => {
    const notes = useNotesContext();
    const { labels } = notes;
    const [isOpen, setIsOpen] = useState(false);
    const root = useRef<HTMLSpanElement>(null);
    const menuId = useId();

    useEffect(() => {
        if (!isOpen) {
            return;
        }

        const dismiss = (event: PointerEvent) => {
            if (!root.current?.contains(event.target as Node)) {
                setIsOpen(false);
            }
        };

        document.addEventListener('pointerdown', dismiss);
        return () => document.removeEventListener('pointerdown', dismiss);
    }, [isOpen]);

    useEffect(() => {
        if (isOpen) {
            root.current?.querySelector<HTMLElement>('[role="menuitem"]:not([disabled])')?.focus();
        }
    }, [isOpen]);

    if (!notes.isAvailable) {
        return null;
    }

    const items = menuItems(labels, notes.isPicking);
    const choose = (action: NotesMenuAction) => {
        setIsOpen(false);
        switch (action) {
            case 'slices': return notes.giveFeedbackOnSlices(dialog);
            case 'screen': return notes.giveFeedbackOnScreen(dialog);
            case 'component': return notes.pointAtSomething(dialog);
            default: return notes.showMyFeedback();
        }
    };

    const onMenuKeyDown = (event: KeyboardEvent<HTMLUListElement>) => {
        if (event.key === 'Escape') {
            event.preventDefault();
            event.stopPropagation();
            setIsOpen(false);
            root.current?.querySelector<HTMLElement>('[data-notes-toggle]')?.focus();
            return;
        }

        const elements = Array.from(event.currentTarget.querySelectorAll<HTMLElement>('[role="menuitem"]'));
        const next = nextMenuIndex(event.key, elements.indexOf(document.activeElement as HTMLElement), elements.length);
        if (next !== undefined) {
            event.preventDefault();
            elements[next].focus();
        }
    };

    return (
        <span ref={root} className={['notes-button', className].filter(Boolean).join(' ')} data-size={size}>
            {size === 'small'
                ? <IconButton variant='ghost' icon={<FeedbackIcon />} aria-label={labels.button.giveFeedback} tooltip={labels.button.giveFeedback} onClick={() => notes.giveFeedback(dialog)} />
                : <Button variant='outline' icon={<FeedbackIcon />} label={labels.button.giveFeedback} onClick={() => notes.giveFeedback(dialog)} />}
            <IconButton
                variant='ghost'
                icon={<ChevronIcon />}
                aria-label={labels.button.moreOptions}
                aria-haspopup='menu'
                aria-expanded={isOpen}
                aria-controls={isOpen ? menuId : undefined}
                data-notes-toggle=''
                onClick={() => setIsOpen(open => !open)} />
            {isOpen && (
                <ul id={menuId} className='notes-button__menu' role='menu' aria-label={labels.button.moreOptions} onKeyDown={onMenuKeyDown}>
                    {items.map(item => (
                        <li key={item.action} role='none'>
                            <button type='button' role='menuitem' className='notes-button__item' disabled={item.disabled} onClick={() => choose(item.action)}>
                                {item.label}
                            </button>
                        </li>
                    ))}
                </ul>
            )}
        </span>
    );
};
