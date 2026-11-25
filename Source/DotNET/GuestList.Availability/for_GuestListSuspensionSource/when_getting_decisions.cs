// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Arc.Availability.Audiences;
using Cratis.Arc.Availability.Suspensions;
using Cratis.Showrunner.GuestList.Availability.given;
using Cratis.Showrunner.GuestList.Contracts.Audiences;
using Cratis.Showrunner.GuestList.Holding;
using Cratis.Showrunner.GuestList.Querying;
using Microsoft.Extensions.Logging.Abstractions;
using AudienceLevel = Cratis.Arc.Availability.Audiences.AudienceLevel;

namespace Cratis.Showrunner.GuestList.Availability.for_GuestListSuspensionSource;

/// <summary>
/// The decisions that apply to a subject are the tenant's and the user's it names, and the system's only for the system
/// caller. Each is tagged with the level it is held at and dated by when the operator decided, a resume included, so the
/// engine can keep the newest per level and path.
/// </summary>
public class when_getting_decisions : Specification
{
    static readonly DateTimeOffset Earlier = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    static readonly DateTimeOffset Later = Earlier.AddHours(1);

    IPartSuspensions _suspensions;
    GuestListSuspensionSource _source;
    IReadOnlyCollection<PartSuspensionDecision> _person;
    IReadOnlyCollection<PartSuspensionDecision> _system;
    IReadOnlyCollection<PartSuspensionDecision> _nobody;

    void Establish()
    {
        _suspensions = Substitute.For<IPartSuspensions>();
        _suspensions.ForSystem(Arg.Any<CancellationToken>()).Returns([new PartDecision("Fleet", true, Earlier)]);
        _suspensions.ForTenant(new TenantId("acme"), Arg.Any<CancellationToken>()).Returns([new PartDecision("Fleet/Reports", true, Earlier), new PartDecision("Fleet/Reports", false, Later)]);
        _suspensions.ForUser(new TenantId("acme"), new UserId("alice"), Arg.Any<CancellationToken>()).Returns([new PartDecision("Fleet/Billing", true, Earlier)]);
        _source = new(_suspensions, new AudienceChangeFeed(new a_clock(), NullLogger<AudienceChangeFeed>.Instance));
    }

    async Task Because()
    {
        _person = await _source.GetDecisions(new AudienceSubject("acme", "alice"));
        _system = await _source.GetDecisions(new AudienceSubject(null, null, IsSystem: true));
        _nobody = await _source.GetDecisions(new AudienceSubject(null, null));
    }

    [Fact] void should_tag_the_tenants_decisions_with_the_tenant_level() => _person.Where(_ => _.Level == AudienceLevel.Tenant).Select(_ => _.Path).ShouldEqual(["Fleet/Reports", "Fleet/Reports"]);
    [Fact] void should_tag_the_users_decisions_with_the_user_level() => _person.Single(_ => _.Level == AudienceLevel.User).Path.ShouldEqual("Fleet/Billing");
    [Fact] void should_return_the_resume_as_well_as_the_suspension() => _person.Where(_ => _.Level == AudienceLevel.Tenant).Select(_ => _.IsSuspended).ShouldEqual([true, false]);
    [Fact] void should_date_each_decision_by_when_the_operator_decided() => _person.Where(_ => _.Level == AudienceLevel.Tenant).Select(_ => _.DecidedAt).ShouldEqual([Earlier, Later]);
    [Fact] void should_not_give_a_person_the_decisions_of_the_system() => _person.ShouldNotContain(_ => _.Level == AudienceLevel.System);
    [Fact] void should_give_the_system_caller_the_decisions_of_the_system() => _system.Single().Path.ShouldEqual("Fleet");
    [Fact] void should_give_the_system_caller_nothing_of_a_tenant() => _system.ShouldNotContain(_ => _.Level == AudienceLevel.Tenant);
    [Fact] void should_give_nobody_nothing() => _nobody.ShouldBeEmpty();
}
#endif
