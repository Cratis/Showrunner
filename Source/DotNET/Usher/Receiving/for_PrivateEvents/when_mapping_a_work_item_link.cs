// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Contracts.WorkItems;

namespace Cratis.Showrunner.Usher.Receiving.for_PrivateEvents;

/// <summary>
/// Only a web address ever reaches the reporter as a link: anything else the desk sends is recorded as no address.
/// </summary>
public class when_mapping_a_work_item_link : Specification
{
    static readonly TenantId _tenant = "fjord-logistics";

    static WorkItemUrl UrlOf(string url) =>
        ((SupportLinkedIssueToWorkItem)PrivateEvents.For(new IssueLinkedToWorkItem(_tenant, new("GitHub", "Cratis/Ledger#42", "Invoices are not sent", url)))!.Value.Event).WorkItem.Url;

    [Fact] void should_keep_an_https_address() => UrlOf("https://github.com/Cratis/Ledger/issues/42").Value.ShouldEqual("https://github.com/Cratis/Ledger/issues/42");
    [Fact] void should_keep_an_http_address() => UrlOf("http://tracker.example.com/42").Value.ShouldEqual("http://tracker.example.com/42");
    [Fact] void should_drop_a_javascript_address() => UrlOf("javascript:alert(1)").ShouldEqual(WorkItemUrl.NotSet);
    [Fact] void should_drop_a_data_address() => UrlOf("data:text/html,<script>alert(1)</script>").ShouldEqual(WorkItemUrl.NotSet);
    [Fact] void should_drop_a_relative_address() => UrlOf("/issues/42").ShouldEqual(WorkItemUrl.NotSet);
    [Fact] void should_keep_no_address() => UrlOf(string.Empty).ShouldEqual(WorkItemUrl.NotSet);
}
#endif
