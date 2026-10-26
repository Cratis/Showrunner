// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { StackFrame } from '../Contracts/Reporting/StackFrame';

const dotNetFrame = /^\s*at\s+(.+?)(?:\s+in\s+(.+):line\s+(\d+))?\s*$/;
const v8FrameWithMethod = /^\s*at\s+(?:async\s+)?(.+?)\s+\((.+?):(\d+)(?::\d+)?\)\s*$/;
const v8FrameWithoutMethod = /^\s*at\s+(?:async\s+)?(.+?):(\d+)(?::\d+)?\s*$/;
const geckoFrame = /^\s*(.*?)@(.+?):(\d+)(?::\d+)?\s*$/;

/**
 * Parses a stack trace into stack frames, top frame first.
 * @remarks
 * Understands the stack of a JavaScript error in Chromium, Node.js, Firefox and Safari, and the .NET stack trace Arc
 * returns for a failing command or query (\`at Type.Method() in File.cs:line 42\`). Lines that are not frames, such
 * as the message line or \`--- End of stack trace from previous location ---\`, are skipped. A frame without a file
 * or line has an empty file and line 0.
 * @param stackTrace The stack trace; nothing gives no frames.
 * @returns The stack frames.
 */
export function parseStackTrace(stackTrace?: string): StackFrame[] {
    if (!stackTrace) {
        return [];
    }

    return stackTrace
        .split(/\r?\n/)
        .map(parseFrame)
        .filter((frame): frame is StackFrame => frame !== undefined);
}

function parseFrame(line: string): StackFrame | undefined {
    let match = v8FrameWithMethod.exec(line);
    if (match) {
        return frame(match[1], match[2], match[3]);
    }

    match = v8FrameWithoutMethod.exec(line);
    if (match && !line.includes('(')) {
        return frame('', match[1], match[2]);
    }

    match = dotNetFrame.exec(line);
    if (match) {
        return frame(match[1], match[2] ?? '', match[3] ?? '0');
    }

    match = geckoFrame.exec(line);
    if (match) {
        return frame(match[1], match[2], match[3]);
    }

    return undefined;
}

function frame(method: string, file: string, line: string): StackFrame {
    return Object.assign(new StackFrame(), {
        method: method.trim(),
        file: file.trim(),
        line: Number.parseInt(line, 10) || 0
    });
}
