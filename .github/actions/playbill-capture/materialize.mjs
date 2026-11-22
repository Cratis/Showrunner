// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Materializes the notes of every published GitHub release that the output folder does not hold yet.
 *
 * For a repository that does not commit its release notes: the notes live in the GitHub releases, and each build writes
 * the missing ones into the folder `Cratis.Showrunner.Playbill` embeds, so the application ships its whole history. A
 * folder that exists is never rewritten. Everything is fetched and staged first and only then moved into place, so a
 * failed fetch leaves the output exactly as it was, and an image that cannot be fetched fails the run rather than leaving
 * an address that stops working.
 *
 * Usage (GH_TOKEN reads the releases and private images):
 *   node materialize.mjs --version <version> --output <dir> [--repository <owner/repo>] [--manifest true|false]
 */
import { promises as fs } from 'node:fs';
import * as path from 'node:path';
import { pathToFileURL } from 'node:url';
import { parseArgs } from 'node:util';
import { appendToManifest, cleanBody, deriveImageFileName, extractRemoteImageUrls, releaseRecord, rewriteImageUrls, validateVersion } from './capture.mjs';
import { fetchResource, readImageBytes } from './fetching.mjs';

async function downloadImages(urls, directory, token, fetchImpl) {
    const urlToFileName = new Map();
    const usedNames = new Set();
    for (const [index, url] of urls.entries()) {
        const response = await fetchResource(url, token, fetchImpl);
        const fileName = deriveImageFileName(url, response.headers.get('content-type'), index, usedNames);
        await fs.writeFile(path.join(directory, fileName), await readImageBytes(response));
        urlToFileName.set(url, fileName);
    }
    return urlToFileName;
}

function versionOf(release) {
    return validateVersion(release.tag_name);
}

async function publishedReleases(repository, version, token, fetchImpl, warn) {
    const releases = [];
    for (let page = 1; ; page++) {
        const response = await fetchResource(`https://api.github.com/repos/${repository}/releases?per_page=100&page=${page}`, token, fetchImpl);
        const batch = await response.json();
        for (const release of batch.filter(candidate => !candidate.draft && candidate.published_at)) {
            try {
                versionOf(release);
                releases.push(release);
            } catch {
                warn(`Skipping release '${release.tag_name}': its tag is not a semantic version.`);
            }
        }
        if (batch.length < 100) {
            break;
        }
    }

    // The release that is being built has just been published. If the API does not show it yet, stop rather than ship without it.
    if (!releases.some(release => versionOf(release) === version)) {
        throw new Error(`The version being built, ${version}, is missing from the GitHub releases.`);
    }

    return releases.sort((left, right) => Date.parse(left.published_at) - Date.parse(right.published_at)
        || versionOf(left).localeCompare(versionOf(right), 'en', { numeric: true }));
}

async function exists(target) {
    return fs.stat(target).then(() => true, error => {
        if (error.code === 'ENOENT') {
            return false;
        }
        throw error;
    });
}

async function stageRelease(release, { staging, output, token, fetchImpl }) {
    const version = versionOf(release);
    if (await exists(path.join(output, version))) {
        return undefined;
    }

    const directory = path.join(staging, version);
    await fs.mkdir(directory);
    const body = cleanBody(release.body ?? '') || 'No release notes were provided for this release.';
    const images = await downloadImages(extractRemoteImageUrls(body), directory, token, fetchImpl);
    await fs.writeFile(path.join(directory, `${version}.md`), `${rewriteImageUrls(body, images).trim()}\n`, 'utf-8');
    await fs.writeFile(path.join(directory, `${version}.json`), releaseRecord({ releasedAt: release.published_at }), 'utf-8');
    return version;
}

/**
 * Writes the notes of every published release that the output folder does not hold yet.
 * @param {object} options - What to materialize.
 * @param {string} options.repository - The repository as owner/name.
 * @param {string} options.version - The version being built; it must be among the published releases.
 * @param {string} options.output - The folder that holds one sub-folder per version.
 * @param {string} [options.token] - Reads the releases and private images.
 * @param {boolean} [options.manifest] - Whether to keep `versions.txt` up to date. Off by default.
 * @param {typeof fetch} [options.fetchImpl] - The fetch to use. Defaults to `fetch`.
 * @param {(message: string) => void} [options.warn] - Reports what was skipped.
 * @returns {Promise<{ written: string[] }>} The versions that were written.
 */
export async function materialize({ repository, version, output, token, manifest = false, fetchImpl = fetch, warn = console.warn }) {
    if (!/^(?!\.{1,2}\/)[\w.-]+\/(?!\.{1,2}$)[\w.-]+$/.test(repository ?? '')) {
        throw new Error('A repository in owner/name form is required.');
    }

    const releases = await publishedReleases(repository, validateVersion(version), token, fetchImpl, warn);
    await fs.mkdir(output, { recursive: true });
    const staging = await fs.mkdtemp(path.join(output, '.playbill-'));
    const written = [];
    try {
        for (const release of releases) {
            const staged = await stageRelease(release, { staging, output, token, fetchImpl });
            if (staged) {
                written.push(staged);
            }
        }

        for (const staged of written) {
            await fs.rename(path.join(staging, staged), path.join(output, staged));
        }

        if (manifest && written.length > 0) {
            const manifestPath = path.join(output, 'versions.txt');
            let text = await fs.readFile(manifestPath, 'utf-8').catch(() => '');
            for (const staged of written) {
                text = appendToManifest(text, staged);
            }
            await fs.writeFile(manifestPath, text, 'utf-8');
        }
    } finally {
        await fs.rm(staging, { recursive: true, force: true });
    }

    return { written };
}

async function main() {
    const { values } = parseArgs({
        options: {
            version: { type: 'string' },
            output: { type: 'string' },
            repository: { type: 'string' },
            manifest: { type: 'string', default: 'false' },
        },
    });
    if (!values.output) {
        throw new Error('--output is required: the folder that holds one sub-folder per version.');
    }
    const { written } = await materialize({
        repository: values.repository || process.env.GITHUB_REPOSITORY,
        version: values.version,
        output: values.output,
        token: process.env.GH_TOKEN,
        manifest: values.manifest === 'true',
    });
    console.log(`Materialized release notes for ${written.length} release(s) through ${validateVersion(values.version)}.`);
}

// Only run main when invoked directly, so the helpers can be imported in tests.
if (process.argv[1] && import.meta.url === pathToFileURL(path.resolve(process.argv[1])).href) {
    main().catch(error => {
        console.error(error.message);
        process.exit(1);
    });
}
