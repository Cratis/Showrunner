// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Reflection;
using Cratis.Showrunner.Notes.Contracts.Responses;

namespace Cratis.Showrunner.Notes.Contracts.for_ContractPayloads;

/// <summary>
/// Every contract names its tenant in the payload, because the receiving side serves many tenants and the application
/// routes each answer into the namespace it names. No contract repeats an event source id in its payload, and nothing in
/// a payload is nullable: a value that is not known is the concept's <c>NotSet</c>.
/// </summary>
public class when_inspecting_their_payloads : Specification
{
    static readonly NullabilityInfoContext _nullability = new();

    Type[] _all = [];
    PropertyInfo[] _properties = [];

    void Because()
    {
        _all = [.. typeof(NoteOnContextGiven).Assembly.GetTypes().Where(type => Attribute.IsDefined(type, typeof(EventTypeAttribute)))];
        _properties = [.. _all.SelectMany(type => type.GetProperties(BindingFlags.Public | BindingFlags.Instance)).Where(property => property.Name != "EqualityContract")];
    }

    [Fact] void should_carry_the_tenant_in_every_contract() => Assert.DoesNotContain(_all, type => type.GetProperty(nameof(TenantId)) is null);
    [Fact] void should_not_repeat_an_event_source_id_in_any_payload() => Assert.DoesNotContain(_all, type => type.GetProperties().Any(property => typeof(EventSourceId).IsAssignableFrom(property.PropertyType)));
    [Fact] void should_not_have_a_nullable_member() => Assert.DoesNotContain(_properties, property => _nullability.Create(property).ReadState == NullabilityState.Nullable);
    [Fact] void should_have_three_responses() => Assert.Equal(3, _all.Count(type => type.Namespace == typeof(NoteAcknowledged).Namespace));
}
#endif
