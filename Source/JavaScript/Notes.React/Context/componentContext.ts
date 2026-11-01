// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { PickedComponent as PickedByUser } from '@cratis/components/ComponentPicker';
import { PickedComponent } from '@cratis/showrunner.notes';
import { LocationParts } from './formatLocation.js';
import { toSliceLocation } from './noteContext.js';

/**
 * The key of what the picker's resolver adds to a picked component: the slice its element belongs to.
 */
export const sliceContextKey = 'slice';

/**
 * Reads the slice location Arc marks an element with, when `<Arc sliceMarkers>` is on.
 * @param element The element.
 * @returns The location of the closest slice marker; undefined when there is none.
 */
export function sliceMarkedOn(element: Element): LocationParts | undefined {
    const marker = element.closest('[data-cratis-module]');
    if (!marker) {
        return undefined;
    }

    return {
        module: marker.getAttribute('data-cratis-module') ?? '',
        feature: marker.getAttribute('data-cratis-feature') ?? '',
        slice: marker.getAttribute('data-cratis-slice') ?? ''
    };
}

/**
 * The picker's `resolve`: tells the slice of a picked component from the closest Arc slice marker.
 * @param element The root element of the picked component.
 * @returns The context to add to the picked component; undefined when the element is in no marked slice.
 */
export function resolveComponentContext(element: Element): Record<string, unknown> | undefined {
    const slice = sliceMarkedOn(element);
    return slice ? { [sliceContextKey]: slice } : undefined;
}

/**
 * Makes the component a note carries from what the picker found.
 * @param picked What the picker found.
 * @param focused The focused location, used when the component's slice is not known.
 * @returns The {@link PickedComponent}.
 */
export function toNotePickedComponent(picked: PickedByUser, focused: LocationParts): PickedComponent {
    const slice = picked.context[sliceContextKey] as LocationParts | undefined;
    return Object.assign(new PickedComponent(), {
        componentName: picked.component,
        componentId: picked.id ?? '',
        part: picked.part ?? '',
        ancestors: picked.ancestors.map(ancestor => ancestor.component),
        location: toSliceLocation(slice ?? focused)
    });
}
