// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Button, ButtonProps } from '@cratis/components/Common';
import { Guid } from '@cratis/fundamentals';
import { PortalIntent } from '@cratis/showrunner.boxoffice';
import { useState } from 'react';
import { useBoxOfficeContext } from '../Provider/BoxOfficeContext.js';
import { useEntitlementsContext } from '../Entitlements/EntitlementsContext.js';

/**
 * Props for {@link PortalButton}.
 */
export interface PortalButtonProps extends Pick<ButtonProps, 'variant' | 'tone' | 'size' | 'className' | 'icon'> {
    /** What the user wants to do in the customer portal. */
    intent: PortalIntent;

    /** The text of the button. */
    label: string;

    /** The offer the user looked at, when they did. */
    offer?: Guid;
}

/**
 * A button that opens the operator's customer portal with a signed link, to buy, try or change plan. It is not shown
 * when the product has no customer portal.
 */
export const PortalButton = ({ intent, label, offer, variant = 'solid', ...rest }: PortalButtonProps) => {
    const { openPortal } = useBoxOfficeContext();
    const { canOpenPortal } = useEntitlementsContext();
    const [isOpening, setIsOpening] = useState(false);

    if (!canOpenPortal) {
        return null;
    }

    const open = async () => {
        setIsOpening(true);
        try {
            await openPortal(intent, offer);
        } finally {
            setIsOpening(false);
        }
    };

    return <Button {...rest} variant={variant} label={label} loading={isOpening} onClick={() => void open()} />;
};
