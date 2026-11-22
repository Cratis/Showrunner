// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Captures the release notes of one released version from a pull request description.
 *
 * For version X it writes `<output>/X/X.md`, downloads the images the description references into `<output>/X/`
 * (rewriting the markdown to relative paths), writes `<output>/X/X.json` with when the release was made and from which pull
 * request, and, and, when asked with `--manifest true`, appends `X` to `<output>/versions.txt` for a frontend that still reads the list. The `Cratis.Showrunner.Playbill` package embeds
 * the output folder in the application when it is built.
 *
 * Usage:
 *   node capture.mjs --version <version> --body-file <path> --output <dir> [--pull <number>] [--released-at <iso date>] [--manifest true|false]  (the manifest is off by default)
 *
 * Image downloads are best-effort: an image that cannot be fetched is left as its original absolute URL, so the note is
 * always produced. Only `https` addresses that answer with an image are kept; anything else is skipped.
 */
import { promises as fs } from 'node:fs';
import * as path from 'node:path';
import { pathToFileURL } from 'node:url';
import { parseArgs } from 'node:util';
import { fetchResource } from './fetching.mjs';

const IMAGE_EXTENSIONS_BY_CONTENT_TYPE = {
    'image/png': 'png',
    'image/jpeg': 'jpg',
    'image/gif': 'gif',
    'image/webp': 'webp',
    'image/svg+xml': 'svg',
};
const MAXIMUM_IMAGE_BYTES = 10 * 1024 * 1024;
const VERSION_PATTERN = /^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(?:-[0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*)?$/;

// Markdown image `![alt](url)` / `![alt](url "title")` and HTML `<img src="url">`.
const markdownImagePattern = /(!\[[^\]]*\]\()([^)\s]+)(\s*(?:"[^"]*")?\s*\))/g;
const htmlImagePattern = /(<img\b[^>]*?\bsrc=)(["'])([^"']+)(\2)/gi;

/**
 * Checks that a version is safe to use as a folder name: a semantic version, with no path in it.
 * @param {string | undefined} version - The version to check.
 * @returns {string} The version, without a leading `v`.
 * @throws When the version is not a semantic version.
 */
export function validateVersion(version) {
    const text = (version ?? '').trim().replace(/^v/i, '');
    if (!VERSION_PATTERN.test(text)) {
        throw new Error(`'${version ?? ''}' is not a semantic version such as 2.3.0 or 2.4.0-beta.1.`);
    }

    return text;
}

/**
 * Remove content that should never ship in user-facing release notes: HTML comments (the pull request template
 * guidance) and any Copilot "Original prompt" transcript block.
 * @param {string} body - The raw pull request description.
 * @returns {string} The cleaned markdown.
 */
export function cleanBody(body) {
    return body
        .replace(/<!--[\s\S]*?-->/g, '')
        .replace(/#{1,6}\s*Original prompt[\s\S]*?(?=\n#{1,6}\s|\n?$)/gi, '')
        .replace(/\n{3,}/g, '\n\n')
        .trim();
}

/**
 * Collect the distinct remote (https) image URLs referenced by a markdown document.
 * @param {string} markdown - The markdown to scan.
 * @returns {string[]} The unique remote image URLs, in first-seen order.
 */
export function extractRemoteImageUrls(markdown) {
    const urls = new Set();
    for (const match of markdown.matchAll(markdownImagePattern)) {
        if (/^https:\/\//i.test(match[2])) {
            urls.add(match[2]);
        }
    }
    for (const match of markdown.matchAll(htmlImagePattern)) {
        if (/^https:\/\//i.test(match[3])) {
            urls.add(match[3]);
        }
    }
    return [...urls];
}

/**
 * Replace every occurrence of an absolute image URL in a markdown document with a relative path.
 * @param {string} markdown - The markdown to rewrite.
 * @param {Map<string, string>} urlToFileName - Map of absolute URL to the local relative file name.
 * @returns {string} The rewritten markdown.
 */
export function rewriteImageUrls(markdown, urlToFileName) {
    let result = markdown;
    for (const [url, fileName] of urlToFileName) {
        result = result.split(url).join(fileName);
    }
    return result;
}

/**
 * Derive a safe, unique file name for a downloaded image.
 * @param {string} url - The image URL.
 * @param {string | null} contentType - The response content type, if known.
 * @param {number} index - The image's index in the document.
 * @param {Set<string>} used - Already-used file names, to keep names unique.
 * @returns {string} A unique file name.
 */
export function deriveImageFileName(url, contentType, index, used) {
    let fromUrl = path.basename(new URL(url).pathname);
    try {
        // An address writes a space as %20; the file is named for what a person would call it.
        fromUrl = decodeURIComponent(fromUrl);
    } catch {
        // A name that is not valid percent-encoding is used as written.
    }
    const hasExtension = /\.[a-z0-9]+$/i.test(fromUrl);
    const contentTypeExtension = IMAGE_EXTENSIONS_BY_CONTENT_TYPE[(contentType ?? '').split(';')[0].trim().toLowerCase()];
    let name = hasExtension
        ? fromUrl
        : `image-${index + 1}.${contentTypeExtension ?? 'png'}`;
    name = name.replace(/[^a-zA-Z0-9._-]/g, '-');
    let candidate = name;
    let suffix = 1;
    while (used.has(candidate)) {
        const extension = path.extname(name);
        candidate = `${path.basename(name, extension)}-${suffix}${extension}`;
        suffix++;
    }
    used.add(candidate);
    return candidate;
}

/**
 * Append a version to the manifest, keeping one version per line and avoiding duplicates.
 * @param {string} manifest - The current manifest text.
 * @param {string} version - The version to append.
 * @returns {string} The updated manifest text.
 */
export function appendToManifest(manifest, version) {
    const versions = manifest.split('\n').map(line => line.trim()).filter(line => line.length > 0);
    if (!versions.includes(version)) {
        versions.push(version);
    }
    return `${versions.join('\n')}\n`;
}

/**
 * Describe how a release was made, for the record written beside its note.
 * @param {{ releasedAt?: string, pull?: string | number }} facts - What is known about the release.
 * @returns {string} The JSON text of the record.
 */
export function releaseRecord({ releasedAt, pull }) {
    const record = { releasedAt: new Date(releasedAt ?? Date.now()).toISOString() };
    const number = Number(pull);
    if (Number.isSafeInteger(number) && number > 0) {
        record.pull = number;
    }
    return `${JSON.stringify(record)}\n`;
}

async function downloadImages(urls, versionDirectory, fetchImage, warn) {
    const urlToFileName = new Map();
    const usedNames = new Set();
    for (const [index, url] of urls.entries()) {
        try {
            const response = await fetchImage(url, { signal: AbortSignal.timeout(30_000) });
            if (!response.ok) {
                warn(`Skipping image (HTTP ${response.status}): ${url}`);
                continue;
            }
            const contentType = response.headers.get('content-type');
            // What is committed is whatever the address answers with, so only an image is kept.
            if (!(contentType ?? '').toLowerCase().startsWith('image/')) {
                warn(`Skipping image (not an image: ${contentType ?? 'no content type'}): ${url}`);
                continue;
            }
            const bytes = Buffer.from(await response.arrayBuffer());
            if (bytes.length > MAXIMUM_IMAGE_BYTES) {
                warn(`Skipping image (larger than ${MAXIMUM_IMAGE_BYTES} bytes): ${url}`);
                continue;
            }
            const fileName = deriveImageFileName(url, contentType, index, usedNames);
            await fs.writeFile(path.join(versionDirectory, fileName), bytes);
            urlToFileName.set(url, fileName);
        } catch (error) {
            warn(`Skipping image (${error.message}): ${url}`);
        }
    }
    return urlToFileName;
}

/**
 * Writes the release notes of a version into the output folder.
 * @param {object} options - What to capture.
 * @param {string} options.version - The version released.
 * @param {string} options.body - The pull request description.
 * @param {string} options.output - The folder that holds one sub-folder per version.
 * @param {string} [options.releasedAt] - When the release was made; now when not given.
 * @param {string | number} [options.pull] - The pull request the release was made from.
 * @param {boolean} [options.manifest] - Whether to keep `versions.txt` up to date. Defaults to false: Playbill reads the folders and needs no manifest, and a shared file is where two releases conflict.
 * @param {typeof fetch} [options.fetchImage] - Downloads an image. Defaults to `fetch`.
 * @param {(message: string) => void} [options.warn] - Reports what was skipped.
 * @returns {Promise<{ directory: string, images: number }>} Where the notes were written and how many images came with them.
 */
export async function capture({ version, body, output, releasedAt, pull, manifest = false, fetchImage = fetch, warn = console.warn }) {
    const safeVersion = validateVersion(version);
    const cleaned = cleanBody(body ?? '') || 'No release notes were provided for this release.';
    const directory = path.join(output, safeVersion);
    await fs.mkdir(directory, { recursive: true });

    const urlToFileName = await downloadImages(extractRemoteImageUrls(cleaned), directory, fetchImage, warn);
    const markdown = `${rewriteImageUrls(cleaned, urlToFileName).trim()}\n`;
    await fs.writeFile(path.join(directory, `${safeVersion}.md`), markdown, 'utf-8');
    await fs.writeFile(path.join(directory, `${safeVersion}.json`), releaseRecord({ releasedAt, pull }), 'utf-8');

    if (manifest) {
        const manifestPath = path.join(output, 'versions.txt');
        const existing = await fs.readFile(manifestPath, 'utf-8').catch(() => '');
        await fs.writeFile(manifestPath, appendToManifest(existing, safeVersion), 'utf-8');
    }

    return { directory, images: urlToFileName.size };
}

async function main() {
    const { values } = parseArgs({
        options: {
            version: { type: 'string' },
            'body-file': { type: 'string' },
            output: { type: 'string' },
            pull: { type: 'string' },
            'released-at': { type: 'string' },
            manifest: { type: 'string', default: 'false' },
        },
    });
    if (!values.output) {
        throw new Error('--output is required: the folder that holds one sub-folder per version.');
    }
    const body = values['body-file'] ? await fs.readFile(values['body-file'], 'utf-8') : '';
    const { directory, images } = await capture({
        version: values.version,
        body,
        output: values.output,
        releasedAt: values['released-at'] || undefined,
        pull: values.pull || undefined,
        manifest: values.manifest === 'true',
        // GH_TOKEN, when set, reads a private repository's images and goes nowhere else.
        fetchImage: url => fetchResource(url, process.env.GH_TOKEN),
    });
    console.log(`Wrote release notes for ${validateVersion(values.version)} (${images} image(s)) to ${directory}`);
}

// Only run main when invoked directly, so the pure helpers can be imported in tests.
if (process.argv[1] && import.meta.url === pathToFileURL(path.resolve(process.argv[1])).href) {
    main().catch(error => {
        console.error(error.message);
        process.exit(1);
    });
}
