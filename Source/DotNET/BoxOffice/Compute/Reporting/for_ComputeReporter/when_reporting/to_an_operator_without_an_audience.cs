// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.BoxOffice.Compute.Reporting.for_ComputeReporter.given;
using Cratis.Showrunner.BoxOffice.given;
using Cratis.Showrunner.BoxOffice.Offering.Declaration;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.BoxOffice.Compute.Reporting.for_ComputeReporter.when_reporting;

public class to_an_operator_without_an_audience : a_compute_reporter
{
    NamedTag[] _tags;

    async Task Because()
    {
        _reporter = new(_client, _logger, Options.Create(new OfferingOptions { OperatorAudience = null }));
        await OrganizationIsKnown();
        var context = ContextFor(2);
        await _reporter.Reached(new ComputeAllowanceReached("agent-runs", new(Tenants.Today), AllowancePeriod.Month, OverageBehavior.Park), context, DeliveryOf(context));
        await ReadOutbox();
        _tags = [.. _outbox.Single().Context.NamedTags];
    }

    [Fact] void should_not_address_the_report() => _tags.ShouldBeEmpty();
}
#endif
