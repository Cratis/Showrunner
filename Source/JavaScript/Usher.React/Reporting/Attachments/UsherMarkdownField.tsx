// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { asCommandFormField, WrappedFieldProps } from '@cratis/arc.react/commands';
import { MarkdownEditor } from '@cratis/components/MarkdownEditor';
import { toast } from '@cratis/components/Notifications';
import { format } from '../../Labels/index.js';
import { useUsherContext } from '../../Provider/UsherContext.js';
import { uploadUsherAttachment } from './uploading.js';

interface UsherMarkdownFieldComponentProps extends WrappedFieldProps<string> {
    placeholder?: string;
    rows?: number;
}

const UsherMarkdownFieldComponent = (props: UsherMarkdownFieldComponentProps) => {
    const { labels } = useUsherContext();

    return (
        <MarkdownEditor
            value={props.value ?? ''}
            onChange={props.onChange}
            onBlur={props.onBlur}
            placeholder={props.placeholder}
            height={`${(props.rows ?? 5) * 1.25 + 4}rem`}
            invalid={props.invalid}
            uploadFile={uploadUsherAttachment}
            onUploadFailed={(file) => toast.error({ title: format(labels.common.uploadFailed, { name: file.name }) })} />
    );
};

/**
 * {@link MarkdownEditor} bound to a `string` property on a report command - lets the reporter paste or drop a file
 * into the description or details, instead of a plain text area.
 *
 * ```tsx
 * <UsherMarkdownField<ReportIssue> value={c => c.description} rows={5} />
 * ```
 */
export const UsherMarkdownField = asCommandFormField(UsherMarkdownFieldComponent, {
    defaultValue: '',
    extractValue: (value: unknown) => value as string,
});
