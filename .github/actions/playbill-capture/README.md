# playbill-capture

A composite GitHub Action that writes a release's notes from the pull request that shipped it into the project that embeds them for [`Cratis.Showrunner.Playbill`](../../../Documentation/Playbill/index.md). It sits next to [`cratis/release-action`](https://github.com/Cratis/release-action): the release action decides the version, publishes the GitHub release and closes the issues it delivered, and `playbill-capture` keeps the same description in the repository so the application can show it.

Full documentation, including every input, the commit behavior and how to move over from a hand-built script, is in [Capture release notes when you ship](../../../Documentation/Playbill/capture-release-notes.md).

## Capture a release

Run it after the release step, giving it the version that step produced:

```yaml
- name: Release
  id: release
  uses: cratis/release-action@v1

- name: Capture release notes
  if: steps.release.outputs.should-publish == 'true'
  uses: Cratis/Showrunner/.github/actions/playbill-capture@v2.19.0
  with:
    version: ${{ steps.release.outputs.version }}
    body: ${{ github.event.pull_request.body }}
    output: Source/Host/ReleaseNotes
    commit: true
```

It writes `<output>/<version>/<version>.md`, the images the note refers to and `<version>.json`. Nothing else is on by default: the commit is `commit: true`, and the `versions.txt` list is `manifest: true`.

## Aggregate unreleased pull requests

A repository that releases from several merged pull requests at once runs the action first with `mode: aggregate`, passes `release-notes` and `version` to the release action, and then captures. The workflow in [the documentation](../../../Documentation/Playbill/capture-release-notes.md#aggregate-unreleased-pull-requests) shows the three steps together.

## Inputs and outputs

| Input | Default | Mode | Meaning |
| --- | --- | --- | --- |
| `mode` | `capture` | both | `capture` or `aggregate`. |
| `version` | | capture | The version released. Anything that is not a semantic version is refused. |
| `output` | | capture | The folder that holds one sub-folder per version. |
| `body` / `body-file` | | both | The notes as text or as a file. A file wins. |
| `pull` | triggering pull request | capture | Recorded in `<version>.json`. |
| `released-at` | now | capture | Recorded in `<version>.json`. |
| `manifest` | `false` | capture | Also keep `versions.txt`. |
| `commit` | `false` | capture | Commit and push the notes. |
| `commit-message` | `Add release notes for {version} [skip ci]` | capture | The commit message. |
| `branch` | `main` | capture | The branch to push to. |
| `trigger`, `base`, `notes-commit`, `repo`, `github-token` | see `action.yml` | aggregate | Where the range starts and what it is read with. |

| Output | Mode | Meaning |
| --- | --- | --- |
| `mode`, `version`, `release-notes`, `missing-label`, `prs` | aggregate | What the unreleased range holds and the notes to release. |
| `directory`, `sha` | capture | The folder written and the commit it is in. |

## Pin it

Pin the action to a release tag, or to the commit of that tag, rather than `main`.

## Develop it

The scripts have their own specs, run by the Build workflow:

```shell
node --test .github/actions/playbill-capture/capture.test.mjs .github/actions/playbill-capture/aggregate.test.mjs
```
