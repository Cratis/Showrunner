---
title: How Playbill works
description: Explains how a release note travels through Playbill, from the shipping pull request through the build and endpoints to components and seen records.
---
A release note makes four journeys. Each one is small, and none of them needs the next to be running.

## Capture

When a pull request is merged and released, its description is the note. The `playbill-capture` action writes it into the project that is the application's entry point:

```text
Source/Host/ReleaseNotes/
  2.3.0/
    2.3.0.md       the note, as it was written
    2.3.0.json     when it was released and from which pull request
    hero.png       every image the note refers to, saved beside it
```

The folder is committed, so the history lives in Git with the code, a build never needs the network and a note can be reviewed and corrected like any other file. Only the note is required. The record is optional, and a folder without one still shows, only without a date. The action is described in [Capture release notes when you ship](./capture-release-notes.md).

## Embedding

The `Cratis.Showrunner.Playbill` package carries MSBuild targets that embed the folder into the application's assembly as resources named `Playbill/<path>`. They are picked up automatically when the package is referenced: nothing is added to the project file. The release history then travels with the assembly, so it is served the same way wherever the application runs, with no dependency on the content root or on what a Docker image copies in.

A project without a `ReleaseNotes` folder builds as before and has an empty history. `PlaybillEmbedReleaseNotes` and `PlaybillReleaseNotesDirectory` change this; see [Add release notes to an application](./add-release-notes-to-an-application.md#choose-what-is-embedded).

At run time the history is read once, the first time it is asked for, because it cannot change while the application runs. A folder is a release only when it is named after a version and holds that version's note. The releases are ordered by semantic versioning, so `1.10.0` comes after `1.9.0` and a prerelease comes before its release. Each note is split at its `##` headings into sections, and its first paragraph, or else the first thing a section lists, becomes the one-line summary.

## Endpoints

`MapPlaybill` serves the history under `/.cratis/playbill/v1`. They are plain HTTP endpoints, so the React components, an operator's tooling or `curl` read the same thing. The list is one request. See the [Endpoint reference](./endpoint-reference.md).

The endpoints require a signed-in user. Release notes can describe behavior that is not meant for everyone, so an application that publishes them opts in to anonymous callers with `AllowAnonymous`.

## What each person has seen

Which releases a person has been told about is a fact about that person, so Playbill records it as an event, `ReleaseNotesSeen`, under a stream of the person's own. `ReleaseNotesStatus.ForMe` is an observable query: it counts the releases newer than the newest one the person has seen, and it updates on every device the person has open when they see more on one. See [Remember what people have seen](./remember-what-people-have-seen.md).

A person who has seen nothing is told about the newest release, not the whole history. What is new for someone who has just arrived is what the application is now.

## Components

`Playbill` is the releases page, `PlaybillDialog` opens by itself with what is new and records that it was read, `PlaybillHistoryDialog` opens the whole history on request, and `PlaybillBadge` is the unread count for the header. All of them draw with Cratis Components, take their colors from the `--cratis-*` tokens, have stable `playbill-*` classes, and can be replaced part by part. See [Customize the release-notes experience](./customize-the-release-notes-experience.md).
