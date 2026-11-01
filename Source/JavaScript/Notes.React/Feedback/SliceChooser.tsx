// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Checkbox, IconButton, TextInput } from '@cratis/components/Common';
import { useId, useState } from 'react';
import { formatLocation, LocationParts } from '../Context/formatLocation.js';
import { CloseIcon } from '../Icons.js';
import { format } from '../Labels/index.js';
import { useNotesLabels } from '../Provider/NotesContext.js';
import { distinctSlices, isChosen, searchSlices, sliceKey, toggleSlice } from './sliceChoice.js';

/**
 * Props for {@link SliceChooser}.
 */
export interface SliceChooserProps {
    /** The slices active on the screen. */
    active: readonly LocationParts[];

    /** Every slice the host knows about, to search over. */
    known: readonly LocationParts[];

    /** The chosen slices. */
    chosen: readonly LocationParts[];

    /** Called with the chosen slices after a change. */
    onChange: (chosen: LocationParts[]) => void;
}

/**
 * Chooses the slices a note is about: the ones active on the screen, and a search over all the slices the host knows.
 */
export const SliceChooser = ({ active, known, chosen, onChange }: SliceChooserProps) => {
    const labels = useNotesLabels();
    const [query, setQuery] = useState('');
    const activeId = useId();
    const resultsId = useId();
    const activeSlices = distinctSlices(active);
    const results = searchSlices(known, query);
    const name = (location: LocationParts) => formatLocation(location, labels.common.locationSeparator, labels.common.unknownLocation);

    const option = (location: LocationParts) => (
        <li key={sliceKey(location)}>
            <Checkbox label={name(location)} checked={isChosen(chosen, location)} onChange={() => onChange(toggleSlice(chosen, location))} />
        </li>
    );

    return (
        <div className='notes-slice-chooser'>
            <section aria-labelledby={activeId}>
                <h3 id={activeId} className='notes-slice-chooser__heading'>{labels.slices.active}</h3>
                {activeSlices.length === 0
                    ? <p className='notes-slice-chooser__empty'>{labels.slices.noneActive}</p>
                    : <ul className='notes-slice-chooser__list'>{activeSlices.map(option)}</ul>}
            </section>
            <section>
                <label className='notes-slice-chooser__heading' htmlFor={`${resultsId}-search`}>{labels.slices.search}</label>
                <TextInput
                    id={`${resultsId}-search`}
                    type='search'
                    value={query}
                    placeholder={labels.slices.searchPlaceholder}
                    aria-controls={resultsId}
                    onChange={value => setQuery(value)} />
                <div id={resultsId} aria-live='polite'>
                    {query.trim().length > 0 && (results.length === 0
                        ? <p className='notes-slice-chooser__empty'>{labels.slices.noMatches}</p>
                        : <ul className='notes-slice-chooser__list' aria-label={labels.slices.results}>{results.map(option)}</ul>)}
                </div>
            </section>
            {chosen.length > 0 && (
                <section>
                    <p className='notes-slice-chooser__heading'>{format(labels.slices.chosen, { count: chosen.length })}</p>
                    <ul className='notes-slice-chooser__chosen'>
                        {chosen.map(location => (
                            <li key={sliceKey(location)} className='notes-slice-chooser__chip'>
                                <span>{name(location)}</span>
                                <IconButton
                                    variant='ghost'
                                    size='small'
                                    icon={<CloseIcon />}
                                    aria-label={format(labels.slices.remove, { slice: name(location) })}
                                    onClick={() => onChange(toggleSlice(chosen, location))} />
                            </li>
                        ))}
                    </ul>
                </section>
            )}
        </div>
    );
};
