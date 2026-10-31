// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Marquee.Contracts.Announcements;

namespace Cratis.Showrunner.Marquee.Contracts.for_ContractEventTypes;

/// <summary>
/// The contracts are shared by stores whose names vary by deployment - every application's own store and every
/// desk store - so no contract may claim an origin store. Each one is public instead, and every observer subscribes
/// explicitly to the store it reads from.
/// </summary>
public class when_inspecting_their_origin : Specification
{
    Type[] _eventTypes = [];

    void Because() => _eventTypes =
    [
        .. typeof(AnnouncementPublished).Assembly.GetTypes().Where(type => Attribute.IsDefined(type, typeof(EventTypeAttribute))),
    ];

    [Fact] void should_find_every_contract() => Assert.Equal(4, _eventTypes.Length);
    [Fact] void should_not_declare_an_origin_store_on_the_assembly() => Assert.False(Attribute.IsDefined(typeof(AnnouncementPublished).Assembly, typeof(EventStoreAttribute)));
    [Fact] void should_not_declare_an_origin_store_on_any_contract() => Assert.DoesNotContain(_eventTypes, type => type.GetEventStoreName() is not null);
    [Fact] void should_declare_every_contract_public() => Assert.DoesNotContain(_eventTypes, type => !Attribute.IsDefined(type, typeof(PublicAttribute), false));
}
#endif
