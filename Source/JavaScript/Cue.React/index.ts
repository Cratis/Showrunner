// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

export type { TourAnchorSpace } from './TourAnchorSpace.js';
export type { TourCallout } from './TourCallout.js';
export type { TourCamera } from './TourCamera.js';
export type { TourChapter } from './TourChapter.js';
export type { TourGesture } from './TourGesture.js';
export type { TourSection } from './TourSection.js';
export type { TourStep } from './TourStep.js';

export { TourAnchor } from './TourAnchor.js';
export type { TourAnchorProps } from './TourAnchor.js';
export { TourCalloutBubble } from './TourCalloutBubble.js';
export type { TourCalloutBubbleProps } from './TourCalloutBubble.js';
export { TourChapterList } from './TourChapterList.js';
export type { TourChapterListProps } from './TourChapterList.js';
export { TourOverlayShell } from './TourOverlayShell.js';
export type { TourOverlayShellProps } from './TourOverlayShell.js';
export { TourPointer } from './TourPointer.js';
export type { TourPointerProps } from './TourPointer.js';
export { TourSelfPacedPanel } from './TourSelfPacedPanel.js';
export type { TourSelfPacedPanelProps } from './TourSelfPacedPanel.js';
export { TourSubtitles } from './TourSubtitles.js';
export type { TourSubtitlesProps } from './TourSubtitles.js';
export { TourTimelineBar } from './TourTimelineBar.js';
export type { TourTimelineBarProps } from './TourTimelineBar.js';

export { useTourPlayback } from './useTourPlayback.js';
export type { TourPlayback } from './useTourPlayback.js';

export { NARRATION_ADVANCE_BEAT_MS, NARRATION_HOLD_LIMIT_MS, advanceTourClock } from './tourClock.js';
export type { TourClockAdvance, TourClockNarration } from './tourClock.js';

export { hasSelfPacedContent, resolveTourMedia, selfPacedMarkdownFor, selfPacedViewFor } from './tourSelfPaced.js';
export type { TourSelfPacedContent, TourSelfPacedView } from './tourSelfPaced.js';

export {
    RESTART_CHAPTER_WINDOW_MS,
    buildTourTimeline,
    chapterIndexAtTime,
    chapterStartMs,
    formatTourTime,
    narrationDurationMs,
    nextChapterTargetMs,
    previousChapterTargetMs,
    sectionIndexAtTime,
    stepDurationMs,
    stepIndexAtTime,
} from './tourTimeline.js';
export type { TimedTourChapter, TimedTourSection, TimedTourStep, TourTimeline } from './tourTimeline.js';

export {
    CHROME_INSETS,
    projectEditorPointToScreen,
    projectToScreen,
    projectToolbarPointToScreen,
    unobstructedCenter,
} from './tourGeometry.js';
export type { BoardTransform, ScreenPoint } from './tourGeometry.js';

export * from './Configuration/index.js';
export * from './Labels/index.js';
export * from './Markdown/index.js';
export * from './Speech/index.js';
export * from './Tours/index.js';
export { CueIcon } from './Icons.js';
export type { CueIconName, CueIconProps } from './Icons.js';
