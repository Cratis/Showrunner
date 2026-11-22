---
title: Endpoint reference
description: The four read endpoints Playbill serves under /.cratis/playbill/v1, with their parameters, what they return, how they are cached and who may call them.
---
`MapPlaybill` serves the release history under `/.cratis/playbill/v1`. The path is versioned: the React components, an operator's tooling and `curl` can rely on it. Every endpoint is a `GET`, and none changes anything.

By default each one requires a signed-in user and answers `401` otherwise. With `AllowAnonymous` set in `PlaybillOptions` they answer anyone. The endpoints are left out of the application's OpenAPI description.

## List the releases

`GET /.cratis/playbill/v1/releases`

| Parameter | Meaning |
| --- | --- |
| `since` | Optional. Only the releases newer than this version. A value that is not a version answers `400`. |

```json
{
  "current": "2.3.0",
  "releases": [
    {
      "version": "2.3.0",
      "releasedAt": "2026-03-02T10:00:00+00:00",
      "pull": "12",
      "summary": "Faster startup and a clearer dashboard.",
      "sections": ["Changed", "Fixed"],
      "assets": ["hero.png"]
    }
  ]
}
```

`releases` is newest first, ordered by semantic versioning. `current` is the version the application is running and is `null` when it is not known. `releasedAt` and `pull` are `null` for a release captured without a record. `summary` is the first paragraph before the sections, or else the first thing a section lists, without markdown or issue references, cut to 200 characters. The list carries no notes: it is what a page needs to draw its list, in one request.

## Read one release

`GET /.cratis/playbill/v1/releases/{version}`

The same address gives two representations, chosen by the `Accept` header, and the response says `Vary: Accept`.

| `Accept` | Response |
| --- | --- |
| anything that does not ask for JSON | `text/markdown`: the note exactly as it was captured. |
| `application/json` | The release with its note split up. |

```json
{
  "version": "2.3.0",
  "releasedAt": "2026-03-02T10:00:00+00:00",
  "pull": "12",
  "summary": "Faster startup and a clearer dashboard.",
  "lead": "Faster startup and a clearer dashboard.\n\n![A hero shot](hero.png)",
  "sections": [
    { "name": "Changed", "markdown": "- The dashboard shows recent activity first. (#12)" },
    { "name": "Fixed", "markdown": "- A crash on startup is gone." }
  ],
  "assets": ["hero.png"],
  "markdown": "..."
}
```

`lead` is what is written before the first `##` heading; it is empty when the note starts with a section. A heading inside fenced code is not a section. A version that has no note answers `404`.

Relative images and links in the markdown point into the release's own folder, which is the next endpoint.

## Read a file of a release

`GET /.cratis/playbill/v1/releases/{version}/{asset}`

A file the note refers to, such as `hero.png`, served with the type of its extension. The name is the path inside the release's folder. Only files that were embedded are served: a path that climbs out of the folder, the note itself and a file of a release that does not exist all answer `404`.

The response carries an `ETag` derived from the build and the path, and `Cache-Control: private, max-age=31536000, immutable`. The history cannot change while the build lives, so the file never needs asking for again. A repeat request with `If-None-Match` answers `304`.

## Read the running version

`GET /.cratis/playbill/v1/current`

```json
{
  "version": "2.3.0",
  "release": { "version": "2.3.0", "summary": "...", "sections": [], "assets": [] },
  "newest": "2.3.0"
}
```

`version` is the running version; `release` is its entry in the list, and is `null` when that version has no note. `newest` is the newest release that has a note. A version that is older than `newest` tells a tool the application is running something that was already superseded in the notes.
