// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import assert from 'node:assert/strict';
import { mkdtempSync, readFileSync, rmSync, existsSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { test } from 'node:test';
import { appendToManifest, capture, cleanBody, deriveImageFileName, extractRemoteImageUrls, releaseRecord, rewriteImageUrls, validateVersion } from './capture.mjs';

const temporary = t => {
    const directory = mkdtempSync(join(tmpdir(), 'playbill-capture-'));
    t.after(() => rmSync(directory, { recursive: true, force: true }));
    return directory;
};
const png = () => new Response(Buffer.from('png-bytes'), { status: 200, headers: { 'content-type': 'image/png' } });
const quiet = () => {};

test('a version is a semantic version, and nothing that could name a path', () => {
    assert.equal(validateVersion('2.3.0'), '2.3.0');
    assert.equal(validateVersion('v2.3.0'), '2.3.0');
    assert.equal(validateVersion(' 2.4.0-beta.1 '), '2.4.0-beta.1');
    assert.equal(validateVersion('2.3.0\n'), '2.3.0');
    for (const bad of ['', undefined, '2.3', '../2.3.0', '2.3.0/../../x', '2.3.0 1', '2.3.0\n1', 'latest', '01.2.3', '2.3.0+build']) {
        assert.throws(() => validateVersion(bad), /not a semantic version/, String(bad));
    }
});

test('comments and a copilot transcript are cleaned out of a description', () => {
    const body = '## Added\n\n<!-- guidance -->\n- One\n\n\n\n## Original prompt\n\nsecret transcript\n\n## Fixed\n\n- Two\n';
    assert.equal(cleanBody(body), '## Added\n\n- One\n\n## Fixed\n\n- Two');
});

test('only https images are collected, once each, in the order they appear', () => {
    const markdown = '![a](https://x.test/a.png) <img src="https://x.test/b.png"> ![a again](https://x.test/a.png) ![local](hero.png) ![plain](http://x.test/c.png)';
    assert.deepEqual(extractRemoteImageUrls(markdown), ['https://x.test/a.png', 'https://x.test/b.png']);
});

test('an image address is replaced by the file it was saved as', () => {
    const markdown = '![a](https://x.test/a.png) and <img src="https://x.test/a.png">';
    assert.equal(rewriteImageUrls(markdown, new Map([['https://x.test/a.png', 'a.png']])), '![a](a.png) and <img src="a.png">');
});

test('an image gets a safe, unique name', () => {
    const used = new Set();
    assert.equal(deriveImageFileName('https://x.test/a b.png', 'image/png', 0, used), 'a-b.png');
    assert.equal(deriveImageFileName('https://y.test/a b.png', 'image/png', 1, used), 'a-b-1.png');
    assert.equal(deriveImageFileName('https://x.test/download', 'image/jpeg', 2, used), 'image-3.jpg');
    assert.equal(deriveImageFileName('https://x.test/download2', undefined, 3, used), 'image-4.png');
});

test('the manifest keeps one version per line and never repeats one', () => {
    assert.equal(appendToManifest('', '1.0.0'), '1.0.0\n');
    assert.equal(appendToManifest('1.0.0\n', '1.1.0'), '1.0.0\n1.1.0\n');
    assert.equal(appendToManifest('1.0.0\n1.1.0\n', '1.1.0'), '1.0.0\n1.1.0\n');
});

test('the record says when and from which pull request, and ignores a pull request that is not a number', () => {
    assert.deepEqual(JSON.parse(releaseRecord({ releasedAt: '2026-03-02T10:00:00Z', pull: '12' })), { releasedAt: '2026-03-02T10:00:00.000Z', pull: 12 });
    assert.deepEqual(JSON.parse(releaseRecord({ releasedAt: '2026-03-02T10:00:00Z', pull: 'abc' })), { releasedAt: '2026-03-02T10:00:00.000Z' });
    assert.ok(!Number.isNaN(Date.parse(JSON.parse(releaseRecord({})).releasedAt)));
});

test('capturing writes the note, the record and the manifest where the Playbill package reads them', async t => {
    const output = temporary(t);
    const result = await capture({ version: '2.3.0', body: '## Added\n\n- A page. (#12)\n', output, releasedAt: '2026-03-02T10:00:00Z', pull: 12, manifest: true });
    assert.equal(result.directory, join(output, '2.3.0'));
    assert.equal(readFileSync(join(output, '2.3.0', '2.3.0.md'), 'utf-8'), '## Added\n\n- A page. (#12)\n');
    assert.deepEqual(JSON.parse(readFileSync(join(output, '2.3.0', '2.3.0.json'), 'utf-8')), { releasedAt: '2026-03-02T10:00:00.000Z', pull: 12 });
    assert.equal(readFileSync(join(output, 'versions.txt'), 'utf-8'), '2.3.0\n');
});

test('capturing a second version keeps the first', async t => {
    const output = temporary(t);
    await capture({ version: '2.3.0', body: '## Added\n\n- A.', output, manifest: true });
    await capture({ version: '2.4.0', body: '## Added\n\n- B.', output, manifest: true });
    assert.equal(readFileSync(join(output, 'versions.txt'), 'utf-8'), '2.3.0\n2.4.0\n');
    assert.ok(existsSync(join(output, '2.3.0', '2.3.0.md')));
});

test('the manifest is left alone unless it is asked for', async t => {
    const output = temporary(t);
    await capture({ version: '2.3.0', body: '## Added\n\n- A.', output });
    assert.ok(!existsSync(join(output, 'versions.txt')));
});

test('an empty description still produces a note', async t => {
    const output = temporary(t);
    await capture({ version: '2.3.0', body: '<!-- nothing -->', output });
    assert.equal(readFileSync(join(output, '2.3.0', '2.3.0.md'), 'utf-8'), 'No release notes were provided for this release.\n');
});

test('an image the description refers to is saved with the note and referred to by name', async t => {
    const output = temporary(t);
    const requested = [];
    const result = await capture({
        version: '2.3.0', body: '![hero](https://x.test/hero.png)', output,
        fetchImage: async url => { requested.push(url); return png(); }, warn: quiet,
    });
    assert.deepEqual(requested, ['https://x.test/hero.png']);
    assert.equal(result.images, 1);
    assert.equal(readFileSync(join(output, '2.3.0', '2.3.0.md'), 'utf-8'), '![hero](hero.png)\n');
    assert.equal(readFileSync(join(output, '2.3.0', 'hero.png'), 'utf-8'), 'png-bytes');
});

test('an image that cannot be fetched, or is not an image, is left as written and never saved', async t => {
    const output = temporary(t);
    const warnings = [];
    const answers = {
        'https://x.test/missing.png': () => new Response('', { status: 404 }),
        'https://x.test/secrets': () => new Response('{"token":"x"}', { status: 200, headers: { 'content-type': 'application/json' } }),
        'https://x.test/broken.png': () => { throw new Error('connection reset'); },
    };
    const body = '![a](https://x.test/missing.png) ![b](https://x.test/secrets) ![c](https://x.test/broken.png)';
    const result = await capture({ version: '2.3.0', body, output, fetchImage: async url => answers[url](), warn: message => warnings.push(message) });
    assert.equal(result.images, 0);
    assert.equal(readFileSync(join(output, '2.3.0', '2.3.0.md'), 'utf-8'), `${body}\n`);
    assert.equal(warnings.length, 3);
    assert.ok(!existsSync(join(output, '2.3.0', 'secrets')));
});

test('a version that is a path is refused before anything is written', async t => {
    const output = temporary(t);
    await assert.rejects(() => capture({ version: '../escape', body: 'x', output }), /not a semantic version/);
    assert.ok(!existsSync(join(output, '..', 'escape')));
});
