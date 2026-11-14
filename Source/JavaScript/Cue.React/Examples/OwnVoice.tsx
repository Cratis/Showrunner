// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
