// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { SliceLocation as ArcSliceLocation } from '@cratis/arc/reflection';

/**
 * Resolves where an error happened: the slice location of the failing command or query when its proxy declares one,
 * otherwise the location given, otherwise {@link ArcSliceLocation.empty}.
 * @param operation The failing command or query, if known.
 * @param location The location the caller knows, such as the one from `useSliceContext()`.
 * @returns The {@link ArcSliceLocation}.
 */
export function resolveLocation(operation?: object, location?: ArcSliceLocation): ArcSliceLocation {
    const declared = operation ? ArcSliceLocation.of(operation) : ArcSliceLocation.empty;
    if (!declared.isEmpty) {
        return declared;
    }

    return location ?? ArcSliceLocation.empty;
}
