// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Arc.Tenancy;
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Identity;

namespace Cratis.Showrunner.Usher.Reporting.Capture.for_CurrentErrorOwner;

public class when_getting_the_owner : Specification
{
    ITenantIdAccessor _tenants;
    bool _signedIn;
    ErrorOwner? _owner;
    bool _anonymous;

    void Establish()
    {
        _tenants = Substitute.For<ITenantIdAccessor>();
        _tenants.Current.Returns(new Cratis.Arc.Tenancy.TenantId("fjord"));
    }

    void Because()
    {
        _signedIn = new CurrentErrorOwner(new SignedInReporter(new FixedPrincipal(Reporters.SignedInAs(Reporters.Subject)), Substitute.For<Access.OnBehalf.IOnBehalfForward>()), _tenants).TryGet(out _owner);
        _anonymous = new CurrentErrorOwner(new SignedInReporter(new FixedPrincipal(Reporters.Anonymous()), Substitute.For<Access.OnBehalf.IOnBehalfForward>()), _tenants).TryGet(out _);
    }

    [Fact] void should_have_an_owner_when_signed_in() => _signedIn.ShouldBeTrue();
    [Fact] void should_be_the_signed_in_user_in_the_current_tenant() => _owner.ShouldEqual(new ErrorOwner("fjord", Reporters.Subject));
    [Fact] void should_have_no_owner_when_anonymous() => _anonymous.ShouldBeFalse();
}
#endif
