// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import assert from 'node:assert/strict';
import { test } from 'node:test';
import { aggregateRelease, bumpVersion, githubOutputs, latestVersion, mergeBodies as mergePullBodies, selectRange, withRetry } from './aggregate.mjs';

// Pull request descriptions in the shape the release-note contract asks for.
const reportsBody = `Reports now load faster by default, so a large report no longer blocks the page while it is drawn.

## Changed

- Reports load in the background for new and existing projects. The previous loader is still available as an explicit fallback in Settings, and will be removed in a later release. (#730)
- Settings saved through the old Settings page no longer keep a project on the previous loader; choose it again in Settings if a project needs it. (#730)
- Ready-to-use hosts now run the faster loader, so the first report of a project starts warm. (#730)
- The report status names the loader a session uses, and shows it as unknown instead of guessing when the host does not report it. (#730)

## Fixed

- The Settings page has one Save button that saves every changed setting, instead of two Save buttons that each saved only part of the page. (#731)
`;
const notesBody = `Sticky notes on the board, the same note the list has (Cratis/Example#752).

## Added

- The board carries sticky notes: add one from the toolbox, write on it, drag it, and delete it with the Delete key. (#752)

## Changed

- The board and the list now use one shared sticky note, toolbox entry and Delete handling, so a note looks and behaves the same on both. A new list note now lands in the middle of the list rather than the middle of the window.
`;
const sha = number => number.toString(16).padStart(40, '0');
const pull = (number, labels = ['patch'], body = `## Fixed\n\n- The board now opens correctly. (#${number})`) =>
    ({ number, labels, body, merged: true, base: { ref: 'main' }, user: { login: 'maintainer' }, merge_commit_sha: sha(number) });
const mergeBodies = (bodies, warn) => mergePullBodies(bodies.map((body, index) => pull(index + 1, [], body)), warn);
const realPulls = [pull(41, ['minor', 'run-integration'], reportsBody), pull(42, ['patch'], notesBody)];
const note = { sha: sha(1), message: 'Add release notes for 0.205.1 [skip ci]' };
const commit = item => ({ sha: item.merge_commit_sha, message: `Merge pull request #${item.number} from Cratis/topic` });
const historyFor = pulls => [...pulls].reverse().map(commit).concat(note);
const plan = (pulls, options = {}) => aggregateRelease({ history: historyFor(pulls), previous: '0.205.1',
    triggerNumber: pulls.at(-1)?.number, repo: 'Cratis/Example', fetchPullRequest: async number => {
        const found = pulls.find(item => item.number === number);
        assert.ok(found, `Unexpected PR lookup: ${number}`);
        return found;
    }, ...options });

let combinedBody;
test('the cancelled #41 run contributes minor intent, sections and issue references alongside #42', async () => {
    const result = await plan(realPulls);
    combinedBody = result.body;
    assert.equal(result.mode, 'aggregate');
    assert.equal(result.version, '0.206.0');
    assert.deepEqual(result.prs.map(item => item.number), [41, 42]);
    assert.deepEqual(result.body.match(/^## .+$/gm), ['## Added', '## Changed', '## Fixed']);
    assert.ok(result.body.includes('(#730)'));
    assert.ok(result.body.includes('(#731)'));
    assert.ok(result.body.indexOf('Reports load') < result.body.indexOf('The board and the list'));
    assert.ok(!result.body.includes('Reports now load'));
    assert.ok(!result.body.includes("Sticky notes on the board, the same"));
    for (const body of [reportsBody, notesBody]) {
        for (const line of body.split('\n').filter(line => line.startsWith('- '))) assert.ok(result.body.includes(line));
    }
});

test('single PR retains its cleaned body including the summary', async () => {
    const result = await plan([realPulls[0]]);
    assert.equal(result.mode, 'single');
    assert.equal(result.body, reportsBody.trim());
    assert.equal(result.version, '0.206.0');
});

test('no-release and Dependabot PRs contribute neither notes nor version intent', async () => {
    const bot = { ...pull(4, ['major']), user: { login: 'dependabot[bot]' } };
    const result = await plan([pull(2), pull(3, ['no-release']), bot]);
    assert.equal(result.mode, 'aggregate'); // The triggering bot must not hide the earlier patch.
    assert.equal(result.version, '0.205.2');
    assert.deepEqual(result.prs.map(item => item.number), [2]);
    assert.ok(!result.body.includes('(#3)'));
    assert.ok(!result.body.includes('(#4)'));
    assert.equal((await plan([bot])).mode, 'none');
    assert.equal((await plan([pull(3, ['no-release'])])).mode, 'none');
});

test('an unlabelled PR contributes sections, a warning, and no bump', async () => {
    const result = await plan([pull(1867, [], 'Lead omitted.\n\n## Changed\n\n- The product now runs on the newest engines.'), pull(1865)]);
    assert.equal(result.version, '0.205.2');
    assert.match(result.body, /newest engines/);
    assert.ok(!result.body.includes('Lead omitted'));
    assert.match(result.warnings.join('\n'), /#1867 is unlabelled/);
    const alone = await plan([pull(1867, [])]);
    assert.equal(alone.mode, 'none');
    assert.equal(alone.missingLabel, true);
});

test('conflicting release intents fail, including no-release combined with a bump', async () => {
    for (const labels of [['minor', 'patch'], ['no-release', 'major']]) {
        await assert.rejects(plan([pull(2, labels)]), /more than one release intent/);
    }
});

test('a PR and its revert in range cancel, including no-release revert PRs', async () => {
    const original = pull(2, ['minor']);
    for (const body of ['Reverts Cratis/Example#2', `This reverts commit ${original.merge_commit_sha}.`]) {
        const result = await plan([original, pull(3, ['no-release'], body), pull(4)]);
        assert.deepEqual(result.prs.map(item => item.number), [4]);
        assert.equal(result.version, '0.205.2');
    }
    const crossRepo = await plan([original, pull(3, ['patch'], 'Reverts Cratis/Other#2\n\n## Fixed\n\n- Another board now opens.')]);
    assert.deepEqual(crossRepo.prs.map(item => item.number), [2, 3]);
    const pairOnly = await plan([original, pull(3, ['patch'], 'Reverts Cratis/Example#2')]);
    assert.equal(pairOnly.mode, 'none');
    assert.equal(pairOnly.body, '');
});

test('duplicate sections collapse in contract order with subheadings and nested bullets unchanged', () => {
    const nested = '### Board\n\n- The board opens. (#20)\n  - Details remain visible.';
    const body = mergeBodies([`Intro\n\n## Summary\n\nIgnored.\n\n## Fixed\n\n${nested}\n\n## Fixed\n\n- A second board opens. (#21)<!-- hidden -->`,
        '## Deprecated\n\n- The old panel is deprecated.\n\n## Security\n\n- Access is restricted.\n\n## Removed\n\n- The obsolete panel is removed.\n\n## Changed\n\n- The panel has clearer labels.\n\n## Added\n\n- A new panel is available.']);
    assert.deepEqual(body.match(/^## .+$/gm), ['## Added', '## Changed', '## Fixed', '## Removed', '## Security', '## Deprecated']);
    assert.ok(body.includes(nested));
    assert.ok(!body.includes('Ignored') && !body.includes('hidden'));
    assert.throws(() => mergeBodies(['Just a lead.', 'Another lead.']), /No release-note sections found for PR #1, PR #2/);
});

test('unknown sections in unlabelled PRs are dropped with a warning identifying the PR and heading', async () => {
    const result = await plan([pull(1867, [], '## Fixed\n\n- The board opens.\n\n## Test plan\n\n- Internal instructions.\n\n## Changed\n\n- The board has clearer labels.'), pull(1865)]);
    assert.match(result.body, /The board opens/);
    assert.match(result.body, /clearer labels/);
    assert.ok(!result.body.includes('Test plan') && !result.body.includes('Internal instructions'));
    assert.ok(result.warnings.some(warning => warning.includes('PR #1867') && warning.includes('"Test plan"')));
    await assert.rejects(plan([pull(10, ['minor'], 'Only a lead.'), pull(11, [], 'Only another lead.')]), /PR #10, PR #11/);
});

test('merged sections retain bullets, subheadings and indented continuations but not standalone prose', () => {
    const bullet = '- The board opens. (#20)\n  Its details remain visible.\n  - Nested behavior stays.';
    const body = mergeBodies([`Lead omitted.\n\n## Fixed\n\nIntroductory prose omitted.\n\n### Board\n\n${bullet}\n\nThanks for reviewing this pull request.\n\n## Changed\n\n- The panel has clearer labels.\n\nInternal follow-up: see a separate task.`, '## Added\n\n- A new panel is available.']);
    assert.ok(body.includes(bullet));
    assert.match(body, /### Board/);
    assert.match(body, /- The panel has clearer labels/);
    for (const prose of ['Lead omitted', 'Introductory prose', 'Thanks for reviewing', 'Internal follow-up']) assert.ok(!body.includes(prose));
});

test('a revert of a revert restores the original PR notes once with the original intent', async () => {
    const original = pull(2, ['minor'], '## Added\n\n- The original behavior is available. (#2)');
    const reverted = pull(3, ['patch'], 'Reverts Cratis/Example#2');
    for (const body of ['Reverts Cratis/Example#3', `This reverts commit ${reverted.merge_commit_sha}.`]) {
        const restore = pull(4, ['patch'], body);
        const result = await plan([original, reverted, restore]);
        assert.deepEqual(result.prs.map(item => item.number), [2]);
        assert.equal(result.version, '0.206.0');
        assert.equal(result.body, original.body);
        assert.equal(result.body.match(/\(#2\)/g).length, 1);
        const revertedAgain = pull(5, ['patch'], 'Reverts Cratis/Example#4');
        assert.deepEqual((await plan([original, reverted, restore, revertedAgain])).prs, []);
        assert.deepEqual((await plan([original, reverted, restore, revertedAgain, pull(6, ['patch'], 'Reverts Cratis/Example#5')])).prs.map(item => item.number), [2]);
    }
});

test('fenced markdown headings are not treated as release sections', () => {
    const example = '```md\n## Fixed\n- Example, not another section.\n```';
    const result = mergeBodies([`## Added\n\n- A panel with this example:\n\n${example}`, '## Fixed\n\n- A board opens.']);
    assert.ok(result.includes(example));
    assert.ok(result.endsWith('## Fixed\n\n- A board opens.'));
});

test('notes commit wins over the tag, and skip-ci/deploy commits are not PRs', async () => {
    const pulls = [pull(2), pull(3)];
    const history = [{ sha: sha(5), message: 'chore(deploy): deploy 0.205.1' }, commit(pulls[1]),
        { sha: sha(4), message: 'Maintenance [skip ci]' }, note, commit(pulls[0])];
    assert.deepEqual(selectRange(history, '0.205.1', sha(2)).commits.map(item => item.sha), [sha(4), sha(3), sha(5)]);
    const result = await plan(pulls, { history });
    assert.deepEqual(result.prs.map(item => item.number), [3]);
});

test('the version tag bounds history when the notes commit is missing', async () => {
    const pulls = [pull(2), pull(3)];
    const result = await plan(pulls, { history: [commit(pulls[1]), commit(pulls[0])], tagSha: sha(2) });
    assert.equal(result.mode, 'single');
    assert.equal(result.lower, sha(2));
    assert.deepEqual(result.prs.map(item => item.number), [3]);
});

test('missing lower bound explicitly falls back to the triggering PR with a warning', async () => {
    const result = await plan(realPulls, { history: realPulls.slice().reverse().map(commit) });
    assert.equal(result.mode, 'single');
    assert.equal(result.fallback, true);
    assert.deepEqual(result.prs.map(item => item.number), [42]);
    assert.match(result.warnings[0], /falling back to single-PR/);
});

test('covered means build coverage, not a release tag on that merge SHA', async () => {
    const covered = pull(2), pending = pull(3, ['minor']);
    const history = [commit(pending), note, commit(covered)];
    const result = await plan([covered, pending], { history, triggerNumber: 2, releasedShas: [sha(999)] });
    // release-action@v1 dist/post/index.js:40242-40248 compares release.target_commitish === sha.
    // It does not check ancestors or the notes-commit build range. Real v0.205.2 and v0.205.3
    // target 52930ab6... and d1bd2698..., respectively, not every merge their builds contained.
    assert.equal(result.mode, 'aggregate');
    assert.equal(result.version, '0.206.0');
    assert.deepEqual(result.prs.map(item => item.number), [3]);
    const empty = await plan([covered], { history: [note, commit(covered)], triggerNumber: 2 });
    assert.equal(empty.mode, 'covered');
    assert.equal(empty.missingLabel, false);
    const rerun = await plan([covered, pending], { history, triggerNumber: 2, releasedShas: [sha(2)] });
    assert.equal(rerun.mode, 'covered');
    assert.equal(rerun.version, '');
});

test('no-release trigger still releases earlier cancelled runs', async () => {
    const result = await plan([pull(2, ['minor']), pull(3, ['no-release'])]);
    assert.equal(result.mode, 'aggregate');
    assert.equal(result.version, '0.206.0');
});

test('invalid API responses and API failures fail closed', async () => {
    for (const update of [{ merged: false }, { base: { ref: 'other' } }, { number: 99 }, { merge_commit_sha: sha(9) }]) {
        await assert.rejects(plan([pull(2)], { fetchPullRequest: async () => ({ ...pull(2), ...update }) }), /not merged|does not match/);
    }
    await assert.rejects(plan([pull(2)], { fetchPullRequest: async () => { throw new Error('API unavailable'); } }), /Could not read PR #2: API unavailable/);
});

test('version selection mirrors stable release precedence and SemVer bumps including 0.x', () => {
    assert.equal(bumpVersion('0.205.1', 'major'), '1.0.0');
    assert.equal(bumpVersion('0.205.1', 'minor'), '0.206.0');
    assert.equal(bumpVersion('1.2.3', 'patch'), '1.2.4');
    assert.throws(() => bumpVersion('0.1', 'minor'), /Invalid previous/);
    assert.throws(() => bumpVersion('1.2.3', 'no-release'), /Invalid release intent/);
    assert.equal(latestVersion([{ tag_name: 'v0.9.0' }, { tag_name: 'v0.10.0' },
        { tag_name: 'v2.0.0', draft: true }, { tag_name: 'v3.0.0', prerelease: true }]), '0.10.0');
    assert.equal(latestVersion([], [{ name: 'latest' }, { name: 'v1' }, { name: 'v1.2' }]), '1.2.0');
    assert.equal(latestVersion([]), '0.0.0');
});

test('soft-wrapped bullet lines without a paragraph break stay with their bullet', () => {
    const body = mergeBodies(['## Fixed\n\n- Opening a board now keeps its notes\nvisible after reconnecting. (#20)\n\nThanks for reviewing.', '## Added\n\n- A new panel is available.']);
    assert.match(body, /- Opening a board now keeps its notes\nvisible after reconnecting\. \(#20\)/);
    assert.doesNotMatch(body, /Thanks for reviewing/);
});

test('a transient GitHub call failure is retried with backoff and then succeeds', () => {
    const waits = [];
    let calls = 0;
    const result = withRetry(() => {
        if (++calls < 3) throw new Error('spawnSync gh ETIMEDOUT');
        return 'ok';
    }, { attempts: 4, delayMs: 10, sleep: wait => waits.push(wait), warn: () => {} });
    assert.equal(result, 'ok');
    assert.equal(calls, 3);
    assert.deepEqual(waits, [10, 20]);
});

test('a persistent GitHub call failure still fails after the last attempt', () => {
    let calls = 0;
    assert.throws(() => withRetry(() => { calls++; throw new Error('spawnSync gh ETIMEDOUT'); },
        { attempts: 3, delayMs: 1, sleep: () => {}, warn: () => {} }), /ETIMEDOUT/);
    assert.equal(calls, 3);
});
