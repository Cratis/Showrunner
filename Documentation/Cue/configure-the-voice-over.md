---
title: Configure the voice-over
description: Connect a text-to-speech service to Cue tours over HTTP with createHttpSpeech, or bring your own voice by implementing the CueSpeech contract.
---

A tour speaks through a text-to-speech service, and which one is the application's choice. Cue does not choose a provider, a voice or a language and holds no key: it asks the voice you configure to say a line, and carries on with subtitles when there is none. The voice is configured once, on `CueProvider`.

## What the tour asks of a voice

A voice is a `CueSpeech`:

| Member | Meaning |
| --- | --- |
| `isAvailable` | Whether a service is there at all. A tour without one plays silently and the voice button says so. |
| `isPreparing` | A line was asked for and its audio is not playing yet. |
| `isSpeaking` | Audio is playing. |
| `hasError` | The last line failed. The tour carries on with subtitles and the voice button offers to try again. |
| `speak(text)` | Says a line, silencing whatever was still being said. |
| `prefetch(text)` | Asks for a line ahead of time, so saying it later does not wait for the service. |
| `stop()` | Silences whatever is being said. |

The tour decides that a line has finished when it was heard preparing or speaking and neither is true any more, so a voice reports those two states truthfully and the tour needs nothing else. It asks for the next two lines only once the current one is audible, so the lookahead never competes with the line the viewer is waiting for.

## Connect a service over HTTP

`createHttpSpeech` builds a voice for a service that answers a request with audio the browser can play, such as an MP3:

```tsx
import { Arc } from '@cratis/arc.react';
import { CratisComponentsProvider } from '@cratis/components';
import { CueProvider, createHttpSpeech } from '@cratis/showrunner.cue.react';
import { ReactNode } from 'react';

// Created once, outside any component.
const speech = createHttpSpeech({
    speakUrl: '/api/speech',
    availableUrl: '/api/speech/available'
});

export const AppShell = ({ children }: { children: ReactNode }) => (
    <CratisComponentsProvider>
        <Arc>
            <CueProvider speech={speech}>
                {children}
            </CueProvider>
        </Arc>
    </CratisComponentsProvider>
);
```

Each line is sent with `POST` as JSON, `{ "text": "..." }`, to `speakUrl`, and the response body is the audio. `availableUrl` is optional. It is asked once with `GET` and must answer `{ "available": true }`; left out, speech is taken to be available and a line that fails is reported as it happens.

| Option | Meaning |
| --- | --- |
| `speakUrl` | Where a line is sent. Required. |
| `availableUrl` | Where availability is asked. Optional. |
| `body` | Builds what is sent for a line, when the service wants more than the text: the voice, the speed, who is speaking. |
| `headers` | Sent with every request, such as an API key for a service that is reached directly. |
| `credentials` | Whether cookies go with the requests. Defaults to `include`, so a service behind the application's own sign-in works. |
| `cacheSize` | How many lines are kept so saying one again does not ask for it again. Defaults to 24. |

The voice is the application's to choose in `body`, never the browser's to name. A key that must stay secret does not belong in `headers` of a browser: put the service behind an endpoint of the application's own and let that endpoint hold the key.

Browsers allow sound only after the viewer has done something. `createHttpSpeech` also gives the provider a function that plays a moment of silence, and the tour calls it from the play and voice buttons, so the lines that follow are allowed. An application that starts a tour from its own button can call it there too.

## Bring your own voice

When the service streams, mixes voices, caches in its own way or plays through its own audio pipeline, the application gives the provider a `CueSpeechConfiguration` of its own: a `useSpeech` hook returning a `CueSpeech`, and optionally an `unlock` function for the viewer's first gesture. This one puts the browser's own voice in that shape:

```tsx
import { CueSpeech, CueSpeechConfiguration } from '@cratis/showrunner.cue.react';
import { useCallback, useState } from 'react';

// A voice the application already has, such as the browser's own, put in the shape the tour asks for.
const useBrowserVoice = (): CueSpeech => {
    const [isSpeaking, setIsSpeaking] = useState(false);

    const stop = useCallback(() => {
        window.speechSynthesis.cancel();
        setIsSpeaking(false);
    }, []);

    const speak = useCallback((text: string) => {
        stop();
        return new Promise<void>(resolve => {
            const line = new SpeechSynthesisUtterance(text);
            line.onstart = () => setIsSpeaking(true);
            line.onend = () => {
                setIsSpeaking(false);
                resolve();
            };
            line.onerror = () => {
                setIsSpeaking(false);
                resolve();
            };
            window.speechSynthesis.speak(line);
        });
    }, [stop]);

    return {
        isAvailable: 'speechSynthesis' in window,
        isPreparing: false,
        isSpeaking,
        hasError: false,
        speak,
        prefetch: () => undefined,
        stop
    };
};

export const browserVoice: CueSpeechConfiguration = { useSpeech: useBrowserVoice };
```

`useSpeech` is called on every render of a tour in the same way, so it must be a stable function and never one created during a render.

## No voice

Leave `speech` out and the tours play silently. The voice button says that the voice-over is not available, the subtitles carry the narration, and the clock runs on the estimate of each step.
