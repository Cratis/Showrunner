#!/usr/bin/env node
// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Publishes the tarballs `run-task-on-workspaces.js pack-version` packed, in the order it recorded in
// publish-order.txt, with npm provenance. It runs nothing from the packages themselves: no install, build or lifecycle
// script runs in the job that holds the OIDC token. A version that is already published is skipped, so a re-run
// completes a partial publish; a package whose dependency failed to publish is not published.

const fs = require('fs');
const path = require('path');
const spawn = require('child_process').spawnSync;

const directory = process.argv[2];
if (!directory) {
    console.error('Usage: publish-tarballs <directory with publish-order.txt and the tarballs>');
    process.exit(1);
}

const lines = fs.readFileSync(path.join(directory, 'publish-order.txt'), 'utf8').split('\n').filter(_ => _.length > 0);
if (lines.length === 0) {
    console.error('publish-order.txt lists nothing to publish');
    process.exit(1);
}

function isPublished(name, version) {
    const result = spawn('npm', ['view', `${name}@${version}`, 'version']);
    return result.status === 0 && result.stdout?.toString().trim() === version;
}

const failed = [];
for (const line of lines) {
    const [name, version, filename] = line.split('\t');
    if (failed.length > 0) {
        console.log(`Skipping '${name}' - an earlier package failed to publish: ${failed.join(', ')}`);
        failed.push(name);
        continue;
    }

    if (isPublished(name, version)) {
        console.log(`Skipping '${name}' - version ${version} is already published`);
        continue;
    }

    console.log(`Publishing '${name}' ${version} from ${filename}`);
    const result = spawn('npm', ['publish', path.resolve(directory, filename), '--provenance', '--ignore-scripts'], { stdio: 'inherit' });
    if (result.status !== 0 && !isPublished(name, version)) {
        console.log(`Error publishing '${name}'`);
        failed.push(name);
    }
}

if (failed.length > 0) {
    console.log(`\n${failed.length} package(s) were not published: ${failed.join(', ')}`);
    process.exit(1);
}
