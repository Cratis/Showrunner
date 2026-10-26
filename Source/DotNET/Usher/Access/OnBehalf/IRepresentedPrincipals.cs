// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Usher.Contracts.Identity;

namespace Cratis.Showrunner.Usher.Access.OnBehalf;

/// <summary>
/// Defines how the user support acts as is described to AuthProxy. AuthProxy forwards the description as the user on
/// every request of the session, so it must carry the roles and claims the application authorizes on.
/// </summary>
/// <remarks>
/// Usher only knows what the reporter shared: their identity provider, subject, name and email. An application whose
/// endpoints require roles registers its own implementation, before or after <c>AddUsher</c>, to add them. The
/// subject must stay the reporter's: an answer naming anybody else is never live.
/// </remarks>
public interface IRepresentedPrincipals
{
    /// <summary>
    /// Describes the reporter support acts as.
    /// </summary>
    /// <param name="reporter">The reporter, as they shared who they are.</param>
    /// <param name="tenant">The tenant the session acts in.</param>
    /// <returns>The <see cref="OnBehalfClientPrincipal"/>.</returns>
    Task<OnBehalfClientPrincipal> For(Reporter reporter, TenantId tenant);
}
