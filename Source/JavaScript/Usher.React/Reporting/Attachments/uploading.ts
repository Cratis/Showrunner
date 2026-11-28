// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/** The endpoint a reporter's pasted or dropped attachment uploads to. Must match `UsherAttachmentEndpoints.Route`. */
export const usherAttachmentUploadPath = '/usher-attachments';

/**
 * The markdown for an uploaded attachment.
 *
 * An image becomes an `<img>` tag rather than `![](...)`, so it can carry a width - a full-size
 * screenshot rendered at its own resolution would push the rest of the report off the page.
 * Everything else becomes an ordinary link with its file name on it.
 * @param fileName The original file name.
 * @param url The address the upload is served from.
 * @param isImage Whether it renders inline.
 * @returns The markdown to put in the report.
 */
export const uploadMarkupFor = (fileName: string, url: string, isImage: boolean): string =>
    isImage
        ? `<img width="640" alt="${fileName}" src="${url}" />`
        : `[${fileName}](${url})`;

/**
 * Uploads a file pasted or dropped into a report's description or details, and returns the markdown
 * that stands for it.
 *
 * Any file, not only images: a log or a screenshot is worth attaching to a report, and refusing it
 * silently is how somebody concludes paste is broken.
 * @param file The file to upload.
 * @returns The markdown for the uploaded file.
 * @throws When the upload is refused, so the editor takes the placeholder out again.
 */
export const uploadUsherAttachment = async (file: File): Promise<string> => {
    const response = await fetch(usherAttachmentUploadPath, {
        method: 'POST',
        headers: {
            'Content-Type': file.type || 'application/octet-stream',
            'X-File-Name': encodeURIComponent(file.name),
        },
        body: file,
    });

    if (!response.ok) {
        throw new Error(`Uploading ${file.name} failed with ${response.status}`);
    }

    const uploaded = await response.json() as { url: string; isImage: boolean };
    return uploadMarkupFor(file.name, uploaded.url, uploaded.isImage);
};
