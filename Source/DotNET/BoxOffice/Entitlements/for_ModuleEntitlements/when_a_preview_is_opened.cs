// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Receiving;

namespace Cratis.Showrunner.BoxOffice.Entitlements.for_ModuleEntitlements;

public class when_a_preview_is_opened : a_tenant_read_model<ModuleEntitlements>
{
    ModuleEntitlements _entitlements;

    async Task Because()
    {
        await Happened(new OperatorOpenedModulePreview("analytics", new(Tenants.Day(7))));
        _entitlements = _scenario.Instance!;
    }

    [Fact] void should_be_a_preview() => _entitlements.AccessTo("analytics", Tenants.Today).ShouldEqual(ModuleAccess.Preview);
    [Fact] void should_end_after_its_last_day() => _entitlements.AccessTo("analytics", Tenants.Day(8)).ShouldEqual(ModuleAccess.None);
}
#endif
