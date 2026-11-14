// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.BoxOffice.Product.Direct.for_DirectConnection.given;

public class InMemoryDirectLinkRecord : IDirectLinkRecord
{
    public DirectLinkState State { get; private set; } = new(DirectLinkStage.NotConfigured, string.Empty);

    public Task<DirectLinkState> Read(CancellationToken cancellationToken) => Task.FromResult(State);

    public Task Redeemed(string endpoint, CancellationToken cancellationToken)
    {
        State = new(DirectLinkStage.Redeemed, endpoint);
        return Task.CompletedTask;
    }

    public Task Verified(CancellationToken cancellationToken)
    {
        State = State with { Stage = DirectLinkStage.Closed };
        return Task.CompletedTask;
    }

    public Task Reopened(CancellationToken cancellationToken)
    {
        State = new(DirectLinkStage.NotConfigured, string.Empty);
        return Task.CompletedTask;
    }
}
#endif
