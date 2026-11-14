// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * The markdown walkthroughs a tour offers for reading it self-paced, keyed by chapter id — the same ids the
 * tour's script gives its chapters. A tour without content simply never offers the self-paced switch.
 */
export type TourSelfPacedContent = Readonly<Record<string, string>>;

/** Everything the tour's chrome needs to draw itself in or out of self-paced reading mode. */
export interface TourSelfPacedView {
    /** Whether the tour is being read self-paced rather than played. */
    isSelfPaced: boolean;

    /** Whether the media playback controls — play, scrubbing, narration and subtitles — are usable. */
    isMediaPlaybackEnabled: boolean;

    /** Whether the chapter list shows where on the clock each chapter starts. */
    areTimingsShown: boolean;

    /** The markdown for the chapter being read, or an empty string when the chapter has none. */
    markdown: string;
}

/**
 * Gets whether a tour has any self-paced content to read at all, which is what decides whether the
 * self-paced switch is offered.
 * @param content The tour's content, or undefined when it has none.
 * @returns True when there is at least one chapter's markdown to show.
 */
export function hasSelfPacedContent(content: TourSelfPacedContent | undefined): boolean {
    return content !== undefined && Object.keys(content).length > 0;
}

/**
 * Gets the markdown to read for a chapter.
 * @param content The tour's content, or undefined when it has none.
 * @param chapterId The chapter being read, or undefined when the tour has no chapters.
 * @returns The chapter's markdown, or an empty string when there is nothing to show for it.
 */
export function selfPacedMarkdownFor(content: TourSelfPacedContent | undefined, chapterId: string | undefined): string {
    if (content === undefined || chapterId === undefined) return '';

    return content[chapterId] ?? '';
}

/**
 * Decides how the tour's chrome behaves for the mode it is in: reading self-paced stops being a media
 * player — the playback controls dim, the chapter timings disappear because there is no clock to time
 * against, and the current chapter's walkthrough is shown — while narrated playback keeps all of it and
 * shows no markdown.
 * @param isSelfPaced Whether the viewer has switched to self-paced reading.
 * @param content The tour's content, or undefined when it has none.
 * @param chapterId The chapter currently reached, or undefined when the tour has no chapters.
 * @returns The {@link TourSelfPacedView} the chrome draws from.
 */
export function selfPacedViewFor(
    isSelfPaced: boolean,
    content: TourSelfPacedContent | undefined,
    chapterId: string | undefined): TourSelfPacedView {
    // Self-paced mode without anything to read would be a silent, frozen tour — so the mode only ever
    // engages when there is content to show.
    const isReading = isSelfPaced && hasSelfPacedContent(content);

    return {
        isSelfPaced: isReading,
        isMediaPlaybackEnabled: !isReading,
        areTimingsShown: !isReading,
        markdown: isReading ? selfPacedMarkdownFor(content, chapterId) : '',
    };
}

// Matches a markdown image (`![alt](url)` or `![alt](url "title")`) and captures the surrounding syntax so
// only the URL is rewritten — the same shape the release-notes rewriter uses.
const markdownImagePattern = /(!\[[^\]]*\]\()([^)\s]+)(\s*(?:"[^"]*")?\s*\))/g;

/**
 * Resolves the media references in a chapter's markdown against the URLs the bundler gave the media files.
 * Content references its screenshots and animations with paths relative to its own folder (`media/name.png`);
 * at runtime those files live wherever the bundler put them, and this maps each reference to that URL. A
 * reference to a file that does not exist (yet) is left untouched, so its alt text still tells the reader
 * what belongs there.
 * @param markdown The chapter's markdown.
 * @param mediaUrls The bundled URL for each media file, keyed by the path the markdown references it with.
 * @returns The markdown with every known media reference resolved.
 */
export function resolveTourMedia(markdown: string, mediaUrls: Readonly<Record<string, string>>): string {
    return markdown.replace(markdownImagePattern, (match, prefix, url, suffix) => {
        const normalized = (url as string).replace(/^\.\//, '');
        const resolved = mediaUrls[normalized];
        return resolved === undefined ? match : `${prefix}${resolved}${suffix}`;
    });
}
