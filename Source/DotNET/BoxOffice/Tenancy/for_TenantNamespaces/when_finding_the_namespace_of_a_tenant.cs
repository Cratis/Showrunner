// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.BoxOffice.given;

namespace Cratis.Showrunner.BoxOffice.Tenancy.for_TenantNamespaces;

public class when_finding_the_namespace_of_a_tenant : Specification
{
    static readonly EventStoreNamespaceName[] _namespaces = [EventStoreNamespaceName.Default, "fjord-logistics", "harbor-freight", "harbor-freight"];

    TenantNamespace _found;
    TenantNamespace _differsInCase;
    TenantNamespace _unknown;
    TenantNamespace _ambiguous;

    void Because()
    {
        _found = TenantNamespaces.Find(_namespaces, Tenants.Tenant);
        _differsInCase = TenantNamespaces.Find(_namespaces, "Fjord-Logistics");
        _unknown = TenantNamespaces.Find(_namespaces, "nobody");
        _ambiguous = TenantNamespaces.Find(_namespaces, "harbor-freight");
    }

    [Fact] void should_find_the_namespace_with_the_exact_name() => _found.Namespace.ShouldEqual(new EventStoreNamespaceName("fjord-logistics"));
    [Fact] void should_match() => _found.Match.ShouldEqual(TenantNamespaceMatch.Found);
    [Fact] void should_not_find_a_name_that_differs_in_case() => _differsInCase.Match.ShouldEqual(TenantNamespaceMatch.Unknown);
    [Fact] void should_not_find_a_name_no_namespace_has() => _unknown.Namespace.ShouldBeNull();
    [Fact] void should_choose_none_when_two_namespaces_share_the_name() => _ambiguous.Match.ShouldEqual(TenantNamespaceMatch.Ambiguous);
    [Fact] void should_not_create_a_namespace_for_an_unknown_tenant() => _unknown.Match.ShouldEqual(TenantNamespaceMatch.Unknown);
}
#endif
