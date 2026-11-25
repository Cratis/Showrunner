// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle.EventSequences;
using Cratis.Chronicle.Schemas;
using Cratis.Chronicle.Testing.Events;

namespace Cratis.Showrunner.BoxOffice.Contracts.Seats.for_SeatTaken;

public class when_appending_a_person : Specification
{
    EventStoreForTesting _eventStore;
    Person _person;
    AppendedEvent _appended;
    JsonSchemaProperty _holderSchema;

    void Establish()
    {
        _eventStore = new();
        _person = new("test-idp", "person-42", "Ada Example", "ada@example.test");
        _holderSchema = Defaults.Instance.JsonSchemaGenerator.Generate(typeof(SeatTaken)).Properties["holder"];
    }

    async Task Because()
    {
        var outbox = _eventStore.GetEventSequence(EventSequenceId.Outbox);
        await outbox.Append("organization-1", new SeatTaken("tenant-1", _person), subject: PersonSubjects.For(_person));
        _appended = (await outbox.GetFromSequenceNumber(EventSequenceNumber.First)).Single();
    }

    void Destroy() => _eventStore.Dispose();

    [Fact] void should_use_the_person_as_compliance_subject() => _appended.Context.Subject.ShouldEqual(PersonSubjects.For(_person));
    [Fact] void should_read_the_person_back() => ((SeatTaken)_appended.Content).Holder.ShouldEqual(_person);
    [Fact] void should_keep_the_organization_as_event_source() => _appended.Context.EventSourceId.Value.ShouldEqual("organization-1");
    [Fact] void should_classify_the_name_as_personal_data() => _holderSchema.Properties["name"].GetComplianceMetadata().Single().metadataType.ShouldEqual("PII");
    [Fact] void should_classify_the_email_as_personal_data() => _holderSchema.Properties["email"].GetComplianceMetadata().Single().metadataType.ShouldEqual("PII");
}
#endif
