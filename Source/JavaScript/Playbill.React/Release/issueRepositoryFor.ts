// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Where the issues of a release are tracked: one address for every release, or a function that answers for each version, for a
 * product whose issues moved to another repository at some point.
 */
export type IssueRepository = string | ((version: string) => string | undefined);

/**
 * Works out the repository a release's issue references belong to.
 * @param issues - What the product gave, if anything.
 * @param version - The version of the release.
 * @returns The address of the repository, or undefined to leave the references as text.
 */
export const issueRepositoryFor = (issues: IssueRepository | undefined, version: string): string | undefined =>
    typeof issues === 'function' ? issues(version) : issues;
