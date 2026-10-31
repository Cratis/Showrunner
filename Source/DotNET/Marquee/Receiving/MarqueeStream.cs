// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle;

namespace Cratis.Showrunner.Marquee.Receiving;

/// <summary>
/// Where the product records the announcements it receives.
/// </summary>
/// <remarks>
/// Announcements are the same for every tenant of the product, so they are recorded once, in the product's own
/// (default) namespace and not in the namespace of each tenant: a tenant created after an announcement was published
/// still sees it while it is active. Each announcement has its own event source, its id, within the product-wide
/// <c>marquee</c> stream.
/// </remarks>
public static class MarqueeStream
{
    /// <summary>
    /// The event source type of everything the product records about the announcements it receives.
    /// </summary>
    public static readonly EventSourceType SourceType = new("marquee");

    /// <summary>
    /// The namespace the product-wide stream lives in: the product's own, which is Chronicle's default namespace.
    /// </summary>
    public static readonly EventStoreNamespaceName Namespace = EventStoreNamespaceName.Default;
}
