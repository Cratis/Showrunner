---
title: Capture release notes when you ship
description: How the playbill-capture GitHub Action writes a release's notes from its pull request into the project that embeds them, with optional aggregation of unreleased pull requests, an optional commit and migration from hand-built scripts.
---
`playbill-capture` is a composite GitHub Action in this repository. It turns the description of the pull request that shipped a release into the folder `Cratis.Showrunner.Playbill` embeds. It has two modes, and everything beyond writing the files is opt-in.

| Mode | When | What it does |
| --- | --- | --- |
| `capture` | After the release action has decided the version. | Writes `<output>/<version>/<version>.md`, saves the images the note refers to beside it, writes `<version>.json`, and optionally commits. |
| `aggregate` | Before the release action, only for a repository that releases from several unreleased pull requests at once. | Finds the pull requests merged since the last release, merges their notes and works out the version. |
| `materialize` | During the build, for a repository that does not commit its notes. | Writes the notes of every published GitHub release the folder does not hold yet, so the build carries the whole history. |

A repository that releases one pull request at a time and commits its notes needs only `capture`. A repository that keeps the notes only in its GitHub releases uses `materialize` instead.

## Capture one release

Run it after the step that releases, giving the version it produced:

```yaml
- name: Capture release notes
  uses: Cratis/Showrunner/.github/actions/playbill-capture@v2.19.0
  with:
    version: ${{ steps.release.outputs.version }}
    body: ${{ github.event.pull_request.body }}
    output: Source/Host/ReleaseNotes
    commit: true
```

The folder it writes is the one the [embedding targets](./add-release-notes-to-an-application.md#choose-what-is-embedded) read, so `output` is the project that is the application's entry point. Pin the action to a release tag, or to the commit of that tag, in your own workflow rather than `main`. It is meant to run next to [`cratis/release-action`](https://github.com/Cratis/release-action): the release action decides the version, publishes the GitHub release and closes the issues it delivered, and `playbill-capture` keeps the same description in the repository for the application to show. The [action's own README](https://github.com/Cratis/Showrunner/blob/main/.github/actions/playbill-capture/README.md) has the short version.

| Input | Default | Meaning |
| --- | --- | --- |
| `version` | | Required. The version released, such as `2.3.0` or `2.4.0-beta.1`. A leading `v` is dropped. Anything that is not a semantic version is refused, because it becomes a folder name. |
| `output` | | Required. The folder that holds one sub-folder per version. |
| `body` / `body-file` | | The note: as text, or as a file. A file wins. Comments and a Copilot "Original prompt" block are removed. |
| `pull` | the triggering pull request | Recorded in `<version>.json`. |
| `released-at` | now | Recorded in `<version>.json`, as an ISO date. |
| `manifest` | `false` | Also keep `versions.txt`. See [Moving from a hand-built setup](#moving-from-a-hand-built-setup). |
| `commit` | `false` | Commit and push the notes. |
| `commit-message` | `Add release notes for {version} [skip ci]` | The message; `{version}` is replaced. |
| `branch` | `main` | The branch to push to. |

Images the note refers to are downloaded and referred to by name, so the note keeps working when the original link expires. Only `https` addresses that answer with an image are kept, up to 10 MB each. An image that cannot be fetched is left as its original address, and the note is still written.

### What the commit does

With `commit: true` the action stages only `output`, commits and pushes. If another release pushed first, it merges that in and tries again, up to five times. It never rebases and never forces. Because each release writes its own folder, two releases do not conflict, which is the reason `versions.txt` is not written unless asked for.

The commit message matters to `aggregate`: it is how the next release finds where this one ended. Keep `commit-message` and `notes-commit` in step if you change either.

Without `commit`, the files are only written and the step reports their folder in the `directory` output. Commit them yourself, or let a later step do it.

## Materialize the releases at build time

A repository that does not want a commit per release keeps its notes in the GitHub releases and writes them into the folder as part of building the image. `materialize` does that for every published release the folder does not hold yet, so the application ships its whole history, and a folder that is already there is never rewritten:

```yaml
- name: Materialize release notes
  uses: Cratis/Showrunner/.github/actions/playbill-capture@v2.19.0
  with:
    mode: materialize
    version: ${{ needs.release.outputs.version }}
    output: Source/Host/ReleaseNotes
    github-token: ${{ github.token }}
```

Each release is written as `<version>/<version>.md` with its images and a `<version>.json` that records when it was published. Everything is fetched and staged before anything is moved into place, so a release or an image that cannot be fetched fails the step and leaves the folder exactly as it was, instead of shipping a history with a hole in it. The version being built has to be among the published releases; if the API does not show it yet, the step stops. Drafts, and tags that are not semantic versions, are skipped.

Give it a token that can read the repository's releases. The token is also used to download the images of a private repository, and it is sent only to GitHub's own addresses, never along a redirect to another host. `capture` does the same with the `github-token` input.

## Aggregate unreleased pull requests

A repository whose releases are cancelled, batched or re-run can have several merged pull requests that no release has covered yet. `aggregate` merges them into one release. Run it before the release action and pass its outputs on:

```yaml
- name: Aggregate unreleased pull requests
  id: aggregate
  uses: Cratis/Showrunner/.github/actions/playbill-capture@v2.19.0
  with:
    mode: aggregate

- name: Release
  id: release
  if: steps.aggregate.outputs.mode == 'single' || steps.aggregate.outputs.mode == 'aggregate'
  uses: cratis/release-action@v1
  with:
    version: ${{ steps.aggregate.outputs.mode == 'aggregate' && steps.aggregate.outputs.version || '' }}
    release-notes: ${{ steps.aggregate.outputs.release-notes }}

- name: Capture release notes
  if: steps.release.outputs.should-publish == 'true'
  uses: Cratis/Showrunner/.github/actions/playbill-capture@v2.19.0
  with:
    version: ${{ steps.release.outputs.version }}
    body: ${{ steps.aggregate.outputs.release-notes }}
    output: Source/Host/ReleaseNotes
    commit: true
```

The checkout before it must have full history (`fetch-depth: 0`) of the branch being released.

`aggregate` starts at the commit that recorded the last release's notes, falling back to that release's tag. It reads each merged pull request in the range, drops `no-release` and Dependabot pull requests, cancels a pull request together with the one that reverts it, takes the highest bump any label asks for, and merges the sections in the contract's order. It fails closed: an unreadable range or a conflicting set of labels is an error, not a guess.

| Input | Default | Meaning |
| --- | --- | --- |
| `trigger` | the pull request of the event | The pull request that triggered the release. |
| `base` | `main` | The branch pull requests merge into. |
| `notes-commit` | `Add release notes for {version} [skip ci]` | The message of the commit that records a version's notes. |
| `repo` | the workflow's repository | As `owner/name`. |
| `github-token` | the workflow's token | Used to read pull requests and releases. |

| Output | Meaning |
| --- | --- |
| `mode` | `single` when one pull request releases, `aggregate` when several do, `covered` when an earlier release already shipped them. |
| `version` | The version to release. |
| `release-notes` | The merged notes. |
| `missing-label` | `true` when a merged pull request carried no release label. |
| `prs` | The pull requests in the range, comma separated. |

A check that only wants to know what is pending, without releasing, runs `aggregate` with `dry-run: true`: it prints the unreleased range and the notes that would be released, writes no body file and sets no outputs.

## Moving from a hand-built setup

Studio and Direct each carry a copy of `generate-version-release-notes.mjs`, and Studio also carries `aggregate-release-notes.mjs`. The action replaces both: the capture script is the same one with the same image handling, and the aggregate script is Studio's with the repository, branch and commit message made inputs.

The folder layout is unchanged, so no existing release note moves. To switch a repository over:

1. Replace the generate step with `capture`, or with `materialize` if the notes are written at build time from the GitHub releases, and the aggregate step with `aggregate`, as above.
2. Add `manifest: true` while a hand-written frontend still reads `versions.txt`. Once the application shows its notes with Playbill, remove it, and delete the file.
3. Add `Cratis.Showrunner.Playbill` to the host, call `AddPlaybill` and `MapPlaybill`, and delete the old release-notes endpoint and its content wiring.
4. Replace the hand-written release-notes component with `Playbill`, `PlaybillDialog` and `PlaybillBadge`, and the "What's new" entry that opened it with `PlaybillHistoryDialog`.

Existing `<version>.md` files work as they are. They have no `<version>.json`, so their releases show without a date until a record is added.
