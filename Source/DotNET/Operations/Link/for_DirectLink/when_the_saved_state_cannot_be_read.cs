// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;
using Microsoft.Extensions.Logging.Abstractions;

namespace Cratis.Showrunner.Operations.Link.for_DirectLink;

public class when_the_saved_state_cannot_be_read : a_direct_link
{
    ILinkStore _unreadable;
    DirectLink _link;

    void Establish()
    {
        _unreadable = Substitute.For<ILinkStore>();
        _unreadable.LoadIdentity(Arg.Any<CancellationToken>()).Returns<Task<LinkIdentity?>>(_ => throw new HttpRequestException("the API server is down"));
        _link = new DirectLink(_options, new HttpDirectClient(new HttpClient(new FakeDirectHandler(_direct)), _options.BaseUrl, Instance), _unreadable, [], null, _time, NullLogger<DirectLink>.Instance);
    }

    async Task Because()
    {
        await _link.Begin(CancellationToken.None);
        await _link.Observe(new Dictionary<string, Alerts.Finding>(), CancellationToken.None);
        await _link.End(CancellationToken.None);
    }

    [Fact] void should_not_claim_because_a_first_claim_would_discard_the_credential() => _direct.Received.ShouldBeEmpty();
    [Fact] void should_not_overwrite_any_saved_state() => _unreadable.DidNotReceive().SaveIdentity(Arg.Any<LinkIdentity>(), Arg.Any<CancellationToken>());
    [Fact] void should_report_that_it_refused() => _unreadable.Received().SaveStatus(Arg.Is<LinkStatus>(status => status.Connection == LinkConnection.Refused), Arg.Any<CancellationToken>());
}
#endif
