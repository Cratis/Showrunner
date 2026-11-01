// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Herald.Contracts.Emails;

namespace Cratis.Showrunner.Herald.Contracts.for_ContractEventTypes;

/// <summary>
/// Herald's facts are recorded in the event log of the application that sends the email, against whatever the email
/// was about, so a host can project them. They are therefore private to that store: public events are only accepted
/// by the outbox. No contract may claim an origin store either, because every application is its own store.
/// </summary>
public class when_inspecting_their_origin : Specification
{
    Type[] _eventTypes = [];

    void Because() => _eventTypes =
    [
        .. typeof(EmailSent).Assembly.GetTypes().Where(type => Attribute.IsDefined(type, typeof(EventTypeAttribute))),
    ];

    [Fact] void should_find_every_contract() => Assert.Equal(3, _eventTypes.Length);
    [Fact] void should_not_declare_an_origin_store_on_the_assembly() => Assert.False(Attribute.IsDefined(typeof(EmailSent).Assembly, typeof(EventStoreAttribute)));
    [Fact] void should_not_declare_an_origin_store_on_any_contract() => Assert.DoesNotContain(_eventTypes, type => type.GetEventStoreName() is not null);
    [Fact] void should_keep_every_contract_private_to_the_event_log() => Assert.DoesNotContain(_eventTypes, type => Attribute.IsDefined(type, typeof(PublicAttribute), false));
}
#endif
