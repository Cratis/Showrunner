// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import assert from 'node:assert/strict';
import { existsSync, mkdirSync, mkdtempSync, readdirSync, readFileSync, rmSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { test } from 'node:test';
import { fetchResource } from './fetching.mjs';
import { materialize } from './materialize.mjs';

const temporary = t => {
    const directory = mkdtempSync(join(tmpdir(), 'playbill-materialize-'));
    t.after(() => rmSync(directory, { recursive: true, force: true }));
    return directory;
};
const quiet = () => {};
const release = (tag, body, published_at, extra = {}) => ({ tag_name: tag, body, published_at, draft: false, ...extra });
const json = value => new Response(JSON.stringify(value), { status: 200, headers: { 'content-type': 'application/json' } });
const png = () => new Response(Buffer.from('png-bytes'), { status: 200, headers: { 'content-type': 'image/png' } });

function service(releases, images = {}, seen = []) {
    return async (url, options) => {
        seen.push({ url, authorization: options?.headers?.Authorization });
        if (url.startsWith('https://api.github.com/repos/Cratis/Direct/releases')) {
            return json(new URL(url).searchParams.get('page') === '1' ? releases : []);
        }
        return images[url]?.() ?? new Response('', { status: 404 });
    };
}

const history = [
    release('v1.0.0', '## Added\n\n- One', '2026-01-01T00:00:00Z'),
    release('v1.1.0', '## Added\n\n- Two ![shot](https://images.test/shot.png)', '2026-02-01T00:00:00Z'),
];

test('every release the folder does not hold is written, with its date and images', async t => {
    const output = join(temporary(t), 'ReleaseNotes');
    const { written } = await materialize({
        repository: 'Cratis/Direct', version: '1.1.0', output,
        fetchImpl: service(history, { 'https://images.test/shot.png': png }), warn: quiet,
    });

    assert.deepEqual(written, ['1.0.0', '1.1.0']);
    assert.equal(readFileSync(join(output, '1.0.0', '1.0.0.md'), 'utf-8'), '## Added\n\n- One\n');
    assert.match(readFileSync(join(output, '1.1.0', '1.1.0.md'), 'utf-8'), /!\[shot\]\(shot\.png\)/);
    assert.equal(readFileSync(join(output, '1.1.0', 'shot.png'), 'utf-8'), 'png-bytes');
    assert.equal(JSON.parse(readFileSync(join(output, '1.1.0', '1.1.0.json'), 'utf-8')).releasedAt, '2026-02-01T00:00:00.000Z');
    assert.deepEqual(readdirSync(output).sort(), ['1.0.0', '1.1.0']);
});

test('a folder that exists is never rewritten, and no manifest is kept unless asked for', async t => {
    const output = join(temporary(t), 'ReleaseNotes');
    mkdirSync(join(output, '1.0.0'), { recursive: true });
    writeFileSync(join(output, '1.0.0', '1.0.0.md'), 'committed\n');

    const { written } = await materialize({ repository: 'Cratis/Direct', version: '1.1.0', output, fetchImpl: service(history, { 'https://images.test/shot.png': png }), warn: quiet });

    assert.deepEqual(written, ['1.1.0']);
    assert.equal(readFileSync(join(output, '1.0.0', '1.0.0.md'), 'utf-8'), 'committed\n');
    assert.equal(existsSync(join(output, 'versions.txt')), false);
});

test('the manifest keeps what was in it and adds what was written, in release order', async t => {
    const output = join(temporary(t), 'ReleaseNotes');
    mkdirSync(output, { recursive: true });
    writeFileSync(join(output, 'versions.txt'), '0.9.0\n');

    await materialize({ repository: 'Cratis/Direct', version: '1.1.0', output, manifest: true, fetchImpl: service(history, { 'https://images.test/shot.png': png }), warn: quiet });

    assert.equal(readFileSync(join(output, 'versions.txt'), 'utf-8'), '0.9.0\n1.0.0\n1.1.0\n');
});

test('an image that cannot be fetched leaves the output exactly as it was', async t => {
    const output = join(temporary(t), 'ReleaseNotes');
    await assert.rejects(materialize({ repository: 'Cratis/Direct', version: '1.1.0', output, fetchImpl: service(history), warn: quiet }), /HTTP 404/);

    assert.deepEqual(readdirSync(output), []);
});

test('the version being built has to be among the published releases', async t => {
    const output = join(temporary(t), 'ReleaseNotes');
    await assert.rejects(materialize({ repository: 'Cratis/Direct', version: '1.2.0', output, fetchImpl: service(history), warn: quiet }), /1\.2\.0, is missing/);
});

test('drafts and tags that are not versions are not written', async t => {
    const output = join(temporary(t), 'ReleaseNotes');
    const warnings = [];
    const releases = [
        ...history.slice(0, 1),
        release('v1.1.0', 'Draft', '2026-02-01T00:00:00Z', { draft: true }),
        release('nightly', 'Not a version', '2026-02-02T00:00:00Z'),
        release('v1.2.0', 'Real', '2026-03-01T00:00:00Z'),
    ];

    const { written } = await materialize({ repository: 'Cratis/Direct', version: '1.2.0', output, fetchImpl: service(releases), warn: message => warnings.push(message) });

    assert.deepEqual(written, ['1.0.0', '1.2.0']);
    assert.equal(warnings.length, 1);
    assert.match(warnings[0], /nightly/);
});

test('a release without notes gets a sentence saying so', async t => {
    const output = join(temporary(t), 'ReleaseNotes');
    await materialize({ repository: 'Cratis/Direct', version: '1.0.0', output, fetchImpl: service([release('v1.0.0', null, '2026-01-01T00:00:00Z')]), warn: quiet });

    assert.equal(readFileSync(join(output, '1.0.0', '1.0.0.md'), 'utf-8'), 'No release notes were provided for this release.\n');
});

test('the repository has to be owner and name', async t => {
    await assert.rejects(materialize({ repository: '../x', version: '1.0.0', output: temporary(t), fetchImpl: service([]), warn: quiet }), /owner\/name/);
});

test('a token is sent to GitHub, and never past a redirect to another host', async () => {
    const seen = [];
    const fetchImpl = async (url, options) => {
        seen.push({ url, authorization: options.headers.Authorization });
        if (url.startsWith('https://private-user-images.githubusercontent.com')) {
            return new Response(null, { status: 302, headers: { location: 'https://signed.example.test/image.png' } });
        }
        return png();
    };

    await fetchResource('https://private-user-images.githubusercontent.com/1/a.png', 'secret', fetchImpl);

    assert.equal(seen[0].authorization, 'Bearer secret');
    assert.equal(seen[1].url, 'https://signed.example.test/image.png');
    assert.equal(seen[1].authorization, undefined);
});

test('a token is not sent to a host that is not GitHub', async () => {
    const seen = [];
    await fetchResource('https://images.test/a.png', 'secret', async (url, options) => { seen.push(options.headers.Authorization); return png(); });

    assert.deepEqual(seen, [undefined]);
});

test('an endless redirect and an unsafe one are refused', async () => {
    await assert.rejects(fetchResource('https://images.test/a.png', undefined, async () => new Response(null, { status: 302, headers: { location: 'https://images.test/a.png' } })), /Too many/);
    await assert.rejects(fetchResource('https://images.test/a.png', undefined, async () => new Response(null, { status: 302, headers: { location: 'file:///etc/passwd' } })), /protocol/);
});
