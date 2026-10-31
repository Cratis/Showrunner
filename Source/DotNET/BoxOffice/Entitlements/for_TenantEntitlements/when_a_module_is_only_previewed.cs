// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Entitlements.for_TenantEntitlements;

public class when_a_module_is_only_previewed : some_entitlements
{
    bool _has;
    ModuleAccess _access;

    async Task Because()
    {
        Given(new OperatorOpenedModulePreview("analytics", new(Tenants.Day(7))));
        _has = await _entitlements.Has("analytics");
        _access = await _entitlements.AccessTo("analytics");
    }

    [Fact] void should_not_count_as_having_the_module() => _has.ShouldBeFalse();
    [Fact] void should_be_a_preview() => _access.ShouldEqual(ModuleAccess.Preview);
}
#endif
