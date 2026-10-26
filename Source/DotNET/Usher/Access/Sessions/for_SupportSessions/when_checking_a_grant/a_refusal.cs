// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Monads;
using Cratis.Showrunner.Usher.Access.Sessions.for_SupportSessions.given;

namespace Cratis.Showrunner.Usher.Access.Sessions.for_SupportSessions.when_checking_a_grant;

public abstract class a_refusal : support_sessions
{
    protected Result<LiveGrant, SupportSessionRefusal> _result;

    protected SupportSessionRefusal Refusal()
    {
        _result.TryGetError(out var refusal);
        return refusal;
    }
}
#endif
