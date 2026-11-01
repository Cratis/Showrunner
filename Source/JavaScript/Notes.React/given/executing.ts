// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { CommandResult } from '@cratis/arc/commands';
import sinon from 'sinon';

// Stubs the execution of a command class and keeps the command instance each execution ran on.
export const stubExecute = <TCommand extends object>(commandType: { prototype: TCommand }, result: () => Promise<CommandResult> = () => Promise.resolve(CommandResult.empty)) => {
    const stub = sinon.stub(commandType.prototype as { execute: () => Promise<CommandResult> }, 'execute').callsFake(result);
    return {
        stub,
        get executions() { return stub.thisValues as unknown as Array<Record<string, unknown>>; }
    };
};
