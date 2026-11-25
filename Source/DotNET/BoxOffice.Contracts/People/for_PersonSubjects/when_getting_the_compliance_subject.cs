// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.BoxOffice.Contracts.People.for_PersonSubjects;

public class when_getting_the_compliance_subject : Specification
{
    Person _person;
    Cratis.Chronicle.Subject _subject;

    void Establish() => _person = new("test-idp", "person-42", "Ada Example", "ada@example.test");

    void Because() => _subject = PersonSubjects.For(_person);

    [Fact] void should_be_stable() => _subject.ShouldEqual(PersonSubjects.For(_person));
    [Fact] void should_hash_the_identity_pair() => _subject.Value.ShouldEqual("box-office-person-5c9f35b7304b49862da777df95d047c4e84678b9e4fc2a35dd10a2add0378ec3");
    [Fact] void should_be_opaque() => _subject.Value.ShouldNotContain(_person.IdentityProviderSubject.Value);
    [Fact] void should_not_expose_the_provider() => _subject.Value.ShouldNotContain(_person.IdentityProvider.Value);
    [Fact] void should_not_use_the_name() => _subject.ShouldEqual(PersonSubjects.For(_person with { Name = "Changed Name" }));
    [Fact] void should_not_use_the_email() => _subject.ShouldEqual(PersonSubjects.For(_person with { Email = "changed@example.test" }));
    [Fact] void should_distinguish_people() => _subject.ShouldNotEqual(PersonSubjects.For(_person with { IdentityProviderSubject = "person-43" }));
    [Fact] void should_distinguish_providers() => _subject.ShouldNotEqual(PersonSubjects.For(_person with { IdentityProvider = "other-idp" }));
    [Fact] void should_distinguish_identity_boundaries() => PersonSubjects.For(_person with { IdentityProvider = "a", IdentityProviderSubject = "bc" }).ShouldNotEqual(PersonSubjects.For(_person with { IdentityProvider = "ab", IdentityProviderSubject = "c" }));
    [Fact] void should_not_confuse_delimiters_in_identity_values() => PersonSubjects.For(_person with { IdentityProvider = "a:b", IdentityProviderSubject = "c" }).ShouldNotEqual(PersonSubjects.For(_person with { IdentityProvider = "a", IdentityProviderSubject = "b:c" }));
}
#endif
