// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { execFileSync } from 'node:child_process';
import { randomUUID } from 'node:crypto';
import { appendFileSync, readFileSync, writeFileSync } from 'node:fs';
import { resolve } from 'node:path';
import { pathToFileURL } from 'node:url';
import { parseArgs } from 'node:util';
import { cleanBody } from './capture.mjs';

const sections = ['Added', 'Changed', 'Fixed', 'Removed', 'Security', 'Deprecated'];
const intents = ['major', 'minor', 'patch', 'no-release'];
const labelsOf = pull => pull.labels.map(label => typeof label === 'string' ? label : label.name);
const isDependabot = pull => ['dependabot[bot]', 'app/dependabot'].includes(pull.user?.login);
const mergeNumber = commit => Number(commit.message.match(/^Merge pull request #(\d+) from\b/)?.[1]) || undefined;

export function bumpVersion(previous, intent) {
    const match = previous.replace(/^v/, '').match(/^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)$/);
    if (!match) throw new Error(`Invalid previous release version: ${previous}`);
    const parts = match.slice(1).map(Number);
    const index = intents.indexOf(intent);
    if (index < 0 || index > 2) throw new Error(`Invalid release intent: ${intent}`);
    // release-action uses SemVer.inc directly, including major 0.x -> 1.0.0.
    parts[index]++;
    parts.fill(0, index + 1);
    if (!parts.every(Number.isSafeInteger)) throw new Error('Version exceeds safe integer range');
    return parts.join('.');
}

export function latestVersion(releases, tags = []) {
    const released = releases.filter(release => !release.draft && !release.prerelease)
        .map(release => release.tag_name.replace(/^v/i, ''))
        .filter(version => /^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)$/.test(version));
    // Mirror release-action's tag bootstrap only when no stable GitHub release exists.
    const candidates = released.length ? released : tags.map(tag => {
        const match = tag.name.replace(/^v/i, '').match(/^(\d+)(?:\.(\d+))?(?:\.(\d+))?/);
        return match ? `${Number(match[1])}.${Number(match[2] ?? 0)}.${Number(match[3] ?? 0)}` : undefined;
    }).filter(Boolean);
    return candidates.sort((left, right) => {
        const a = left.split('.').map(Number), b = right.split('.').map(Number);
        return b[0] - a[0] || b[1] - a[1] || b[2] - a[2];
    })[0] ?? '0.0.0';
}

/**
 * The message of the commit that records the release notes of a version, so a release can find where the last one ended.
 * @param {string} version - The version the notes are for.
 * @returns {string} The commit message.
 */
export const defaultNotesCommit = version => `Add release notes for ${version} [skip ci]`;
export function selectRange(history, previous, tagSha, notesCommit = defaultNotesCommit) {
    let index = history.findIndex(commit => commit.message.split('\n')[0] === notesCommit(previous));
    if (index < 0 && tagSha) index = history.findIndex(commit => commit.sha === tagSha);
    if (index < 0) return { commits: [], lower: undefined };
    return { commits: history.slice(0, index).reverse(), lower: history[index].sha };
}

function cleanPullBody(pull) {
    const body = pull.body ?? '';
    if (typeof body !== 'string') throw new Error(`PR #${pull.number} has non-text release notes`);
    return cleanBody(body);
}

export function mergeBodies(pulls, warn = console.warn) {
    if (pulls.length === 1) return cleanPullBody(pulls[0]);
    const collected = new Map(sections.map(section => [section, []]));
    for (const pull of pulls) {
        let current, fence, inBullet = false, previousBlank = false;
        for (const line of cleanPullBody(pull).split('\n')) {
            const afterBlank = previousBlank;
            previousBlank = !line.trim();
            const marker = line.match(/^\s{0,3}(`{3,}|~{3,})/);
            if (marker && (!fence || (marker[1][0] === fence[0] && marker[1].length >= fence.length))) {
                fence = fence ? undefined : marker[1];
            }
            const heading = !fence && !marker && line.match(/^##\s+(.+?)\s*#*\s*$/);
            if (heading) {
                const name = heading[1];
                if (name !== 'Summary' && !collected.has(name)) warn(`PR #${pull.number}: dropping unknown release-note section "${name}".`);
                current = collected.has(name) ? name : undefined;
                inBullet = false;
            } else if (current) {
                if (fence || marker) {
                    if (inBullet) collected.get(current).push(line);
                } else if (!line.trim()) {
                    collected.get(current).push(line);
                } else if (/^###\s/.test(line)) {
                    collected.get(current).push(line);
                    inBullet = false;
                } else if (/^\s*(?:[-+*]|\d+[.)])\s+/.test(line)) {
                    collected.get(current).push(line);
                    inBullet = true;
                } else if (inBullet && (/^(?: {2,}|\t)/.test(line) || !afterBlank)) {
                    // Indented continuation, or a soft-wrapped line of the bullet with no paragraph break.
                    collected.get(current).push(line);
                } else {
                    // Unindented prose after a paragraph break is outside a list item, including a trailing PR sign-off.
                    inBullet = false;
                }
            }
        }
        for (const lines of collected.values()) if (lines.length && lines.at(-1) !== '') lines.push('');
    }
    const merged = [...collected].filter(([, lines]) => lines.some(line => line.trim()))
        .map(([section, lines]) => `## ${section}\n\n${lines.join('\n').trim()}`).join('\n\n');
    if (pulls.length && !merged) throw new Error(`No release-note sections found for ${pulls.map(pull => `PR #${pull.number}`).join(', ')}`);
    return cleanBody(merged);
}

function withNetRevertEffects(pulls, repo) {
    const effects = new Map(), totals = new Map();
    for (const [index, pull] of pulls.entries()) {
        const reference = (pull.body ?? '').match(/^Reverts ([\w.-]+\/[\w.-]+)#(\d+)\.?\s*$/m);
        const revertedSha = (pull.body ?? '').match(/This reverts commit ([0-9a-f]{40})\b/i)?.[1];
        const original = pulls.slice(0, index).find(candidate =>
            ((reference && reference[1].toLowerCase() === repo.toLowerCase() && candidate.number === Number(reference[2])) ||
             (revertedSha && candidate.merge_commit_sha === revertedSha)));
        const prior = effects.get(original?.number);
        const effect = prior ? { root: prior.root, sign: -prior.sign } : { root: pull.number, sign: 1 };
        effects.set(pull.number, effect);
        totals.set(effect.root, (totals.get(effect.root) ?? 0) + effect.sign);
    }
    // Revert chains contribute alternating signs; publish the original notes once if restored.
    return pulls.filter(pull => effects.get(pull.number).root === pull.number && totals.get(pull.number) > 0);
}

export async function aggregateRelease({ history, previous, tagSha, triggerNumber, fetchPullRequest, repo = process.env.GITHUB_REPOSITORY ?? 'owner/repo', releasedShas = [], base = 'main', notesCommit = defaultNotesCommit }) {
    const range = selectRange(history, previous, tagSha, notesCommit);
    const warnings = [];
    const candidates = range.lower ? range.commits : history.slice(0, history.findIndex(commit => mergeNumber(commit)) + 1);
    let numbers = candidates.filter(commit => !/\[skip ci\]|^chore\(deploy\)/m.test(commit.message)).map(mergeNumber).filter(Boolean);
    if (!range.lower) {
        warnings.push(`No notes commit or first-parent tag for ${previous}; falling back to single-PR behavior.`);
        numbers = [triggerNumber ?? numbers.at(-1)].filter(Boolean);
    }
    const pulls = [];
    for (const number of new Set(numbers)) {
        let pull;
        try { pull = await fetchPullRequest(number); }
        catch (error) { throw new Error(`Could not read PR #${number}: ${error.message}`, { cause: error }); }
        if (pull.number !== number || !pull.merged || pull.base?.ref !== base) throw new Error(`PR #${number} is not merged into ${base}`);
        const commit = candidates.find(candidate => mergeNumber(candidate) === number);
        if (commit && pull.merge_commit_sha !== commit.sha) throw new Error(`PR #${number} does not match merge commit ${commit.sha}`);
        if (!isDependabot(pull) && intents.filter(intent => labelsOf(pull).includes(intent)).length > 1) {
            throw new Error(`PR #${number} carries more than one release intent`);
        }
        pulls.push(pull);
    }
    const included = withNetRevertEffects(pulls, repo).filter(pull => !isDependabot(pull) && !labelsOf(pull).includes('no-release'));
    for (const pull of included) if (!intents.some(intent => labelsOf(pull).includes(intent))) warnings.push(`PR #${pull.number} is unlabelled; including sections without a version bump.`);
    const intent = intents.slice(0, 3).find(label => included.some(pull => labelsOf(pull).includes(label)));
    const covered = range.lower && triggerNumber && !numbers.includes(triggerNumber);
    // A covered PR's changes can be in an earlier build without its merge SHA being a release
    // target. release-action's existsForSha compares target_commitish === sha, not commit ranges.
    const rerun = Boolean(triggerNumber && releasedShas.includes(history.find(commit => mergeNumber(commit) === triggerNumber)?.sha));
    const single = included.length === 1 && (!triggerNumber || included[0].number === triggerNumber);
    const mode = rerun ? 'covered' : intent ? (single ? 'single' : 'aggregate') : covered ? 'covered' : 'none';
    return { mode, version: intent && !rerun ? bumpVersion(previous, intent) : '', intent,
        prs: included, body: mergeBodies(included, warning => warnings.push(warning)), warnings,
        lower: range.lower, head: history[0]?.sha, fallback: !range.lower,
        missingLabel: !covered && pulls.some(pull => pull.number === triggerNumber && !isDependabot(pull) && !intents.some(label => labelsOf(pull).includes(label))) };
}

const run = (command, args, timeout = 30_000) => execFileSync(command, args, { encoding: 'utf8', timeout, maxBuffer: 32 * 1024 * 1024 }).trim();
const pause = milliseconds => Atomics.wait(new Int32Array(new SharedArrayBuffer(4)), 0, 0, milliseconds);

// A GitHub API call from a busy runner times out now and then (run 39042089974 failed its whole release on one
// `spawnSync gh ETIMEDOUT` after a three hour queue). Retrying a read is always safe; the result only ever feeds a
// release-notes aggregation, so a persistent failure still fails closed after the last attempt.
export function withRetry(action, { attempts = 4, delayMs = 5_000, sleep = pause, warn = console.warn } = {}) {
    for (let attempt = 1; ; attempt++) {
        try { return action(); }
        catch (error) {
            if (attempt >= attempts) throw error;
            const wait = delayMs * 2 ** (attempt - 1);
            warn(`GitHub call failed (attempt ${attempt}/${attempts}): ${error.message}; retrying in ${wait / 1000}s.`);
            sleep(wait);
        }
    }
}

const gh = args => withRetry(() => run('gh', args, 90_000));
const api = endpoint => JSON.parse(gh(['api', endpoint]));
const pages = endpoint => JSON.parse(gh(['api', '--paginate', '--slurp', endpoint])).flat();

export function githubOutputs(result, bodyFile) {
    const delimiter = `notes_${randomUUID()}`;
    return `mode=${result.mode}\nversion=${result.version}\nprs=${result.prs.map(pull => pull.number).join(',')}\n` +
        `missing-label=${result.missingLabel}\nbody-file=${bodyFile}\nrelease-notes<<${delimiter}\n${result.body}\n${delimiter}\n`;
}

async function main() {
    const { values: args } = parseArgs({ options: {
        'dry-run': { type: 'boolean' }, head: { type: 'string', default: 'HEAD' }, previous: { type: 'string' },
        repo: { type: 'string', default: process.env.GITHUB_REPOSITORY },
        base: { type: 'string', default: 'main' }, 'notes-commit': { type: 'string', default: 'Add release notes for {version} [skip ci]' },
        trigger: { type: 'string' }, 'body-file': { type: 'string' },
    } });
    if (!/^[\w.-]+\/[\w.-]+$/.test(args.repo ?? '')) throw new Error('Invalid repository; pass --repo owner/name or run in GitHub Actions.');
    const head = run('git', ['rev-parse', '--verify', `${args.head}^{commit}`]);
    const records = run('git', ['log', '--first-parent', '--format=%H%x00%B%x00', head]).split('\0');
    const history = [];
    for (let index = 0; index + 1 < records.length; index += 2) history.push({ sha: records[index].trim(), message: records[index + 1].trim() });
    const releases = args.previous && args['dry-run'] ? [] : pages(`repos/${args.repo}/releases?per_page=100`);
    const tags = run('git', ['tag', '--list']).split('\n').filter(Boolean).map(name => ({ name }));
    const previous = args.previous?.replace(/^v/, '') ?? latestVersion(releases, tags);
    const tag = tags.find(candidate => candidate.name === `v${previous}`);
    const tagSha = tag ? run('git', ['rev-parse', '--verify', `refs/tags/${tag.name}^{commit}`]) : undefined;
    const event = process.env.GITHUB_EVENT_PATH ? JSON.parse(readFileSync(process.env.GITHUB_EVENT_PATH, 'utf8')) : undefined;
    const triggerNumber = Number(args.trigger ?? event?.pull_request?.number) || undefined;
    const notesCommit = version => args['notes-commit'].split('{version}').join(version);
    const result = await aggregateRelease({ history, previous, tagSha, triggerNumber, repo: args.repo, base: args.base, notesCommit,
        fetchPullRequest: number => api(`repos/${args.repo}/pulls/${number}`),
        releasedShas: args['dry-run'] ? [] : releases.map(release => release.target_commitish) });
    // Single mode deliberately retains release-action's event-snapshot PR behavior and body.
    if (result.mode === 'single' && event?.pull_request?.number === result.prs[0]?.number) result.body = cleanBody(event.pull_request.body ?? '');
    for (const warning of result.warnings) console.warn(`Warning: ${warning}`);
    console.log(`Range: ${result.lower ?? '(missing lower bound; single PR)'}..${result.head}`);
    console.log(`PRs: ${result.prs.map(pull => `#${pull.number} [${labelsOf(pull).join(', ') || 'unlabelled'}]`).join(', ') || '(none)'}`);
    console.log(`Mode: ${result.mode}\nVersion: ${result.version || '(none)'}\n\n${result.body}`);
    if (args['dry-run']) return;
    const bodyFile = resolve(args['body-file'] ?? '.ai-work/release-body.md');
    writeFileSync(bodyFile, result.body, 'utf8');
    if (process.env.GITHUB_OUTPUT) appendFileSync(process.env.GITHUB_OUTPUT, githubOutputs(result, bodyFile));
}

if (process.argv[1] && import.meta.url === pathToFileURL(resolve(process.argv[1])).href) {
    main().catch(error => { console.error(error.message); process.exitCode = 1; });
}
