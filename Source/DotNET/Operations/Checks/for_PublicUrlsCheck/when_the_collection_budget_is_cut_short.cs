// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using Cratis.Showrunner.Operations.given;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.Checks.for_PublicUrlsCheck;

public class when_the_collection_budget_is_cut_short : a_check
{
    StubHttpHandler _handler;

    protected override IWatchdogCheck Check => new PublicUrlsCheck(new HttpClient(_handler), [new Uri("https://example.com/")], _time);

    void Establish() => _handler = StubHttpHandler.Answering(HttpStatusCode.OK);

    async Task Because() => await Run(budget: TimeSpan.FromSeconds(5));

    [Fact] void should_not_probe_without_the_time_for_a_whole_attempt() => _handler.Requests.ShouldBeEmpty();
    [Fact] void should_say_how_many_attempts_were_made_rather_than_call_it_clean() => _findings.Single().Text.ShouldContain("probed 0/3 attempts; the rest were skipped: collection budget exhausted");
    [Fact] void should_not_report_an_outage() => Keys.ShouldNotContain("url:https://example.com/");
}
#endif
