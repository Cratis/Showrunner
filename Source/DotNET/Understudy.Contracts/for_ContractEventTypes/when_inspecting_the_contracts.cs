// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Understudy.Contracts.Backups;

namespace Cratis.Showrunner.Understudy.Contracts.for_ContractEventTypes;

/// <summary>
/// Stage Manager publishes what it does through the outbox of an event store, so every contract is a public event, and none claims an origin store.
/// </summary>
public class when_inspecting_the_contracts : Specification
{
    Type[] _eventTypes = [];

    void Because() => _eventTypes =
    [
        .. typeof(BackupTaken).Assembly.GetTypes().Where(type => Attribute.IsDefined(type, typeof(EventTypeAttribute))),
    ];

    [Fact] void should_find_the_seven_facts() => Assert.Equal(7, _eventTypes.Length);
    [Fact] void should_name_the_facts() => Assert.Equal(
        ["BackupDownloaded", "BackupFailed", "BackupTaken", "RehearsalFailed", "RehearsalPassed", "RestoreCompleted", "RestoreFailed"],
        _eventTypes.Select(type => type.Name).Order(StringComparer.Ordinal));
    [Fact] void should_publish_every_contract() => Assert.All(_eventTypes, type => Assert.True(Attribute.IsDefined(type, typeof(PublicAttribute), false), type.Name));
    [Fact] void should_not_declare_an_origin_store_on_the_assembly() => Assert.False(Attribute.IsDefined(typeof(BackupTaken).Assembly, typeof(EventStoreAttribute)));
    [Fact] void should_not_declare_an_origin_store_on_any_contract() => Assert.DoesNotContain(_eventTypes, type => type.GetEventStoreName() is not null);
}
#endif
