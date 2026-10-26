// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Monads;
using Cratis.Showrunner.Usher.given;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Usher.Access.OnBehalf.for_OnBehalfTokens.given;

public class on_behalf_tokens : Specification
{
    protected static readonly DateTimeOffset Now = new(2026, 10, 26, 12, 0, 0, TimeSpan.Zero);
    protected static readonly Uri Target = new("https://portal.internal/.cratis/on-behalf/grants?subject=s&issue=i&actor=a&session=x");
    protected static readonly OnBehalfSessionFacts Session = new("a-session", "tenant-one", Guid.NewGuid().ToString(), (string)Reporters.Subject, "support-person", "aad");

    protected AuthProxyTokens _authProxy;
    protected ScenarioTime _time;
    protected OnBehalfTokens _tokens;
    protected OnBehalfOptions _options;
    protected Result<OnBehalfAssertion, OnBehalfTokenRejection> _result;

    protected OnBehalfCall GrantCheck => new(OnBehalfPaths.GrantCheckPurpose, "GET", string.Empty, OnBehalfPaths.Grants, ReadOnlyMemory<byte>.Empty);

    void Establish()
    {
        _authProxy = new();
        _time = new(Now);
        _options = new();
        var options = Substitute.For<IOptionsMonitor<UsherOptions>>();
        options.CurrentValue.Returns(new UsherOptions { ApplicationName = "Ledger", OnBehalf = _options });
        _tokens = new(AuthProxyTokens.ArcOptions(_authProxy), options, new OnBehalfTokenReplays(_time), _time);
    }

    void Destroy() => _authProxy.Dispose();

    protected string GrantCheckToken(string purpose = OnBehalfPaths.GrantCheckPurpose) =>
        _authProxy.ForCall(purpose, HttpMethod.Get, Target, [], Session, Now);

    protected Task Verify(string token, OnBehalfCall? call = null) =>
        Assign(_tokens.Verify($"Bearer {token}", call ?? GrantCheck));

    protected async Task Assign(Task<Result<OnBehalfAssertion, OnBehalfTokenRejection>> verification) => _result = await verification;

    protected OnBehalfTokenRejection Rejection()
    {
        _result.TryGetError(out var rejection);
        return _result.IsSuccess ? (OnBehalfTokenRejection)(-1) : rejection;
    }
}
#endif
