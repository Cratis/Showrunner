// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { PresentationChange } from '@cratis/components/ComponentPicker';
import { Guid } from '@cratis/fundamentals';
import { ISuggestComponentPropertyChange, NoteContext, PickedComponent } from '@cratis/showrunner.notes';

/**
 * Turns the value of a presentation property into the text a suggestion carries.
 * @param value The value; undefined when the property was not set.
 * @returns The text; empty when the property was not set.
 */
export function propertyValueText(value: unknown): string {
    return value === undefined || value === null ? '' : String(value);
}

/**
 * Makes the values of one `SuggestComponentPropertyChange` per changed property.
 * @param changes What the user changed in the property editor.
 * @param context What the user was looking at.
 * @param component The picked component.
 * @param noteId The id the whole picking session shares.
 * @returns The command values, in the order of the changes.
 */
export function propertySuggestions(
    changes: readonly PresentationChange[],
    context: NoteContext,
    component: PickedComponent,
    noteId: Guid): ISuggestComponentPropertyChange[] {
    return changes.map(change => ({
        noteId,
        context,
        component,
        property: change.property,
        currentValue: propertyValueText(change.from),
        suggestedValue: propertyValueText(change.to)
    }));
}
