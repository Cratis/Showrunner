// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Message } from '@cratis/components/Display';
import { PortalIntent } from '@cratis/showrunner.boxoffice';
import { ReactNode } from 'react';
import { PortalButton } from '../Portal/PortalButton.js';
import { useBoxOfficeLabels } from '../Provider/BoxOfficeContext.js';
import { useEntitlementsContext } from './EntitlementsContext.js';
import { requiresModuleModel } from './requiresModuleModel.js';

/**
 * Props for {@link RequiresModule}.
 */
export interface RequiresModuleProps {
    /** The module whose content this is. */
    module: string;

    /**
     * What to do when the tenant does not have the module: `hide` shows the fallback, which is nothing unless given;
     * `explain` says why it is not there, and offers to try or buy it. Defaults to `hide`.
     */
    mode?: 'hide' | 'explain';

    /** What to show instead of the content in `hide` mode. Nothing when not given. */
    fallback?: ReactNode;

    /** The content of the module. */
    children?: ReactNode;
}

/**
 * Shows its content only when the tenant has the module, in full or on trial. Nothing is shown while the entitlements
 * load, so the content never flashes up and is taken away. The backend refuses the module's commands and queries on its
 * own; this is what keeps the user from meeting the refusal.
 */
export const RequiresModule = ({ module, mode = 'hide', fallback, children }: RequiresModuleProps) => {
    const entitlements = useEntitlementsContext();
    const labels = useBoxOfficeLabels();

    if (entitlements.isLoading) {
        return null;
    }

    const model = requiresModuleModel(entitlements, module, labels);
    if (model.hasModule) {
        return <>{children}</>;
    }

    if (mode === 'hide') {
        return <>{fallback}</>;
    }

    return (
        <div className='boxoffice-requires-module'>
            <Message severity='info'>
                <div className='boxoffice-requires-module__text'>
                    <strong>{model.title}</strong>
                    <span>{model.text}</span>
                </div>
            </Message>
            <div className='boxoffice-requires-module__actions'>
                {model.canTry && <PortalButton intent={PortalIntent.try} label={labels.requiresModule.try} variant='outline' />}
                <PortalButton intent={PortalIntent.buy} label={labels.requiresModule.buy} />
            </div>
        </div>
    );
};
