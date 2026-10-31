// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Marquee, MarqueeMarkdownProps } from '@cratis/showrunner.marquee.react';

const PlainText = ({ source }: MarqueeMarkdownProps) => <span>{source}</span>;

export const MarqueeWithPlainText = () => <Marquee markdown={PlainText} />;
