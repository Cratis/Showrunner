// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.BoxOffice.Contracts.for_ContractPayloads;

/// <summary>
/// Every tenant-scoped contract names its tenant in the payload, because Direct serves many tenants and a product
/// routes each event into the namespace it names. The product-wide contracts concern no single tenant, and no
/// contract repeats an event source id in its payload.
/// </summary>
public class when_inspecting_their_payloads : Specification
{
    static readonly string[] _productWide = [nameof(OfferingDeclared), nameof(OfferPublished), nameof(OfferWithdrawn)];

    Type[] _tenantScoped = [];
    Type[] _productWideTypes = [];
    Type[] _all = [];

    void Because()
    {
        _all = [.. typeof(TenantRegistered).Assembly.GetTypes().Where(type => Attribute.IsDefined(type, typeof(EventTypeAttribute)))];
        _tenantScoped = [.. _all.Where(type => !_productWide.Contains(type.Name))];
        _productWideTypes = [.. _all.Where(type => _productWide.Contains(type.Name))];
    }

    [Fact] void should_have_three_product_wide_contracts() => Assert.Equal(3, _productWideTypes.Length);
    [Fact] void should_carry_the_tenant_in_every_tenant_scoped_contract() => Assert.DoesNotContain(_tenantScoped, type => type.GetProperty(nameof(TenantId)) is null);
    [Fact] void should_not_carry_a_tenant_in_a_product_wide_contract() => Assert.DoesNotContain(_productWideTypes, type => type.GetProperty(nameof(TenantId)) is not null);
    [Fact] void should_not_repeat_an_event_source_id_in_any_payload() => Assert.DoesNotContain(_all, type => type.GetProperties().Any(property => typeof(EventSourceId).IsAssignableFrom(property.PropertyType)));
}
#endif
