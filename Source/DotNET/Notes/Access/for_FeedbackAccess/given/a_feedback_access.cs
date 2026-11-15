// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Showrunner.Notes.Access.given;
using Cratis.Showrunner.Notes.given;
using Cratis.Showrunner.Notes.Identity;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Notes.Access.for_FeedbackAccess.given;

/// <summary>
/// Access decided for the giver the specifications act as, in an application that lets only granted users give feedback
/// unless a specification says otherwise.
/// </summary>
public class a_feedback_access : Specification
{
    protected FeedbackGiversStub _givers;
    protected FeedbackAccess _access;

    /// <summary>
    /// Gets who may give feedback in the application.
    /// </summary>
    protected virtual FeedbackAccessMode AccessMode => FeedbackAccessMode.GrantedUsers;

    /// <summary>
    /// Gets the principal that asks; the giver by default.
    /// </summary>
    protected virtual ClaimsPrincipal Principal => Givers.SignedInAs(Givers.UserId);

    void Establish()
    {
        _givers = new();
        _access = new(
            Options.Create(new NotesOptions { AccessMode = AccessMode }),
            new SignedInGiver(new FixedPrincipal(Principal)),
            _givers);
    }
}
#endif
