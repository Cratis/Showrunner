// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Fills the placeholders of a label, such as `{count}`, with values. A placeholder without a value is left as it is.
 * @param template The label.
 * @param values The values by placeholder name.
 * @returns The label with its placeholders filled in.
 */
export function format(template: string, values: Record<string, string | number>): string {
    return template.replace(/\{(\w+)\}/g, (placeholder, name: string) =>
        Object.hasOwn(values, name) ? String(values[name]) : placeholder);
}
